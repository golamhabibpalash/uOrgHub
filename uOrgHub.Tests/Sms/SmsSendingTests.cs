using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Messages.Commands;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Models.Enums;
using uOrgHub.Sms.Providers;

namespace uOrgHub.Tests.Sms;

public class SmsSendServiceTests
{
    [Fact]
    public async Task Creates_one_queued_message_per_unique_recipient_and_prices_the_send()
    {
        var h = new SmsHarness();
        h.AddGateway(rate: 0.40m);

        var (request, duplicate) = await h.SendService().CreateAsync(
            ["01712345678", "+8801712345678", "01812345678"], new string('a', 200), "key-0001", "Manual", null);

        duplicate.Should().BeFalse();
        request.RecipientCount.Should().Be(2);
        request.SegmentsPerRecipient.Should().Be(2);
        request.TotalSegments.Should().Be(4);
        request.EstimatedCost.Should().Be(1.60m);
        (await h.Db.Set<SmsMessage>().CountAsync(m => m.Status == SmsMessageStatus.Queued)).Should().Be(2);
    }

    [Fact]
    public async Task The_same_idempotency_key_returns_the_first_send_instead_of_sending_again()
    {
        var h = new SmsHarness();
        h.AddGateway();
        var service = h.SendService();

        var (first, _) = await service.CreateAsync(["01712345678"], "Hello", "key-dup-01", "Manual", null);
        var (second, duplicate) = await service.CreateAsync(["01712345678"], "Hello", "key-dup-01", "Manual", null);

        duplicate.Should().BeTrue();
        second.Id.Should().Be(first.Id);
        (await h.Db.Set<SmsMessage>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Invalid_numbers_are_reported_and_nothing_is_queued()
    {
        var h = new SmsHarness();
        h.AddGateway();

        var act = () => h.SendService().CreateAsync(["01712345678", "12345"], "Hi", "key-bad-01", "Manual", null);

        (await act.Should().ThrowAsync<ValidationException>()).Which.Errors.Should().ContainSingle(e => e.Contains("12345"));
        (await h.Db.Set<SmsMessage>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Without_an_active_gateway_sending_is_refused()
    {
        var h = new SmsHarness();
        h.AddGateway(isDefault: false);

        var act = () => h.SendService().CreateAsync(["01712345678"], "Hi", "key-nogw-1", "Manual", null);

        await act.Should().ThrowAsync<AppException>().WithMessage("*No active SMS gateway*");
    }

    [Fact]
    public async Task Bulk_needs_its_own_permission_counted_after_normalising()
    {
        var h = new SmsHarness();
        h.AddGateway();
        var handler = new SendSmsCommandHandler(h.SendService());

        // The same number written two ways is one recipient — allowed without bulk.
        await handler.Handle(new SendSmsCommand(new SendSmsDto
        {
            Recipients = ["01712345678", "+880 1712-345678"], Message = "Hi", IdempotencyKey = "key-one-001",
        }, null, AllowBulk: false), default);

        var bulk = () => handler.Handle(new SendSmsCommand(new SendSmsDto
        {
            Recipients = ["01712345678", "01812345678"], Message = "Hi", IdempotencyKey = "key-two-001",
        }, null, AllowBulk: false), default);

        (await bulk.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(403);
    }
}

public class SmsDispatcherTests
{
    private static async Task<(SmsHarness H, SmsMessage Message)> QueueOne()
    {
        var h = new SmsHarness();
        h.AddGateway(rate: 0.25m);
        var (request, _) = await h.SendService().CreateAsync(["01712345678"], "Hello", "key-" + Guid.NewGuid(), "Manual", null);
        return (h, request.Messages.Single());
    }

    [Fact]
    public async Task Accepted_messages_are_submitted_and_costed_at_the_gateway_rate()
    {
        var (h, message) = await QueueOne();

        (await h.Dispatcher().ProcessQueuedAsync(10, default)).Should().Be(1);

        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Submitted);
        message.ProviderMessageId.Should().Be("P-1");
        message.Cost.Should().Be(0.25m);
        message.CostIsActual.Should().BeFalse();
        h.Fake.Sent.Single().Recipient.Should().Be("8801712345678");
    }

    [Fact]
    public async Task Rejected_messages_fail_without_retry()
    {
        var (h, message) = await QueueOne();
        h.Fake.Script.Enqueue(_ => new SmsSendResult(SmsSendOutcome.Rejected, ErrorCode: "1007", ErrorMessage: "Insufficient balance."));

        await h.Dispatcher().ProcessQueuedAsync(10, default);

        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Failed);
        message.ErrorCode.Should().Be("1007");
        message.NextAttemptAt.Should().BeNull();
    }

    [Fact]
    public async Task Retryable_failures_are_requeued_with_backoff_then_given_up()
    {
        var (h, message) = await QueueOne();
        for (var i = 0; i < 3; i++)
            h.Fake.Script.Enqueue(_ => new SmsSendResult(SmsSendOutcome.Retryable, ErrorCode: "UNREACHABLE", ErrorMessage: "Could not reach."));

        await h.Dispatcher().ProcessQueuedAsync(10, default);
        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Queued);
        message.NextAttemptAt.Should().BeAfter(DateTime.UtcNow);

        // Not due yet: nothing is sent.
        (await h.Dispatcher().ProcessQueuedAsync(10, default)).Should().Be(0);

        for (var attempt = 2; attempt <= 3; attempt++)
        {
            message.NextAttemptAt = DateTime.UtcNow.AddSeconds(-1);
            await h.Db.SaveChangesAsync();
            await h.Dispatcher().ProcessQueuedAsync(10, default);
            await h.Db.Entry(message).ReloadAsync();
        }

        message.Status.Should().Be(SmsMessageStatus.Failed);
        message.Attempts.Should().Be(3);
        h.Fake.Sent.Should().HaveCount(3);
    }

    /// <summary>A timeout may have sent the message: retrying could text the person twice.</summary>
    [Fact]
    public async Task A_timeout_is_undetermined_and_never_resent()
    {
        var (h, message) = await QueueOne();
        h.Fake.Script.Enqueue(_ => new SmsSendResult(SmsSendOutcome.Undetermined, ErrorCode: "TIMEOUT", ErrorMessage: "No reply."));

        await h.Dispatcher().ProcessQueuedAsync(10, default);
        await h.Dispatcher().ProcessQueuedAsync(10, default);

        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Undetermined);
        h.Fake.Sent.Should().ContainSingle();
    }

    [Fact]
    public async Task Messages_interrupted_mid_send_become_undetermined_not_resent()
    {
        var (h, message) = await QueueOne();
        message.Status = SmsMessageStatus.Sending;
        await h.Db.SaveChangesAsync();

        (await h.Dispatcher().RecoverInterruptedAsync(default)).Should().Be(1);
        await h.Dispatcher().ProcessQueuedAsync(10, default);

        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Undetermined);
        h.Fake.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task Queued_messages_on_a_disabled_gateway_fail_with_a_reason()
    {
        var (h, message) = await QueueOne();
        var gateway = await h.Db.Set<SmsGateway>().SingleAsync();
        gateway.IsEnabled = false;
        await h.Db.SaveChangesAsync();

        await h.Dispatcher().ProcessQueuedAsync(10, default);

        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Failed);
        message.ErrorMessage.Should().Contain("disabled");
        h.Fake.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task Delivery_reports_mark_messages_delivered_with_the_actual_charge()
    {
        var (h, message) = await QueueOne();
        await h.Dispatcher().ProcessQueuedAsync(10, default);
        message.SubmittedAt = DateTime.UtcNow.AddMinutes(-5);
        await h.Db.SaveChangesAsync();
        h.Fake.Delivery = new SmsDeliveryResult(true, SmsMessageStatus.Delivered, "Sent", 0.30m);

        (await h.Dispatcher().CheckDeliveryAsync(10, default)).Should().Be(1);

        await h.Db.Entry(message).ReloadAsync();
        message.Status.Should().Be(SmsMessageStatus.Delivered);
        message.Cost.Should().Be(0.30m);
        message.CostIsActual.Should().BeTrue();
    }
}
