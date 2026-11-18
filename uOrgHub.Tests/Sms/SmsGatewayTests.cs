using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Dashboard.Queries;
using uOrgHub.Sms.Features.Gateways.Commands;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Models.Enums;
using uOrgHub.Sms.Providers;

namespace uOrgHub.Tests.Sms;

public class SmsGatewayCommandTests
{
    private static CreateSmsGatewayCommandHandler Create(SmsHarness h)
        => new(h.Db, h.Registry, h.Protector, h.Runtime, NullLogger<CreateSmsGatewayCommandHandler>.Instance);

    private static UpdateSmsGatewayCommandHandler Update(SmsHarness h)
        => new(h.Db, h.Registry, h.Protector, h.Runtime, NullLogger<UpdateSmsGatewayCommandHandler>.Instance);

    private static CreateSmsGatewayDto NewDto(string name = "Primary") => new()
    {
        Name = name,
        ProviderKey = "fake",
        Credentials = new() { ["api_key"] = "plain-secret-1" },
        Settings = new() { ["region"] = "bd", ["not_a_field"] = "dropped" },
    };

    [Fact]
    public async Task First_gateway_becomes_active_and_credentials_are_never_returned()
    {
        var h = new SmsHarness();

        var dto = await Create(h).Handle(new CreateSmsGatewayCommand(NewDto()), default);

        dto.IsDefault.Should().BeTrue();
        dto.Credentials.Should().ContainSingle(c => c.Key == "api_key" && c.IsSet);
        dto.Settings.Should().ContainKey("region").And.NotContainKey("not_a_field");
        System.Text.Json.JsonSerializer.Serialize(dto).Should().NotContain("plain-secret-1");
        (await h.Db.Set<SmsGateway>().SingleAsync()).CredentialsCipher.Should().NotContain("plain-secret-1");
    }

    [Fact]
    public async Task A_required_credential_must_be_supplied()
    {
        var h = new SmsHarness();
        var dto = NewDto();
        dto.Credentials.Clear();

        var act = () => Create(h).Handle(new CreateSmsGatewayCommand(dto), default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Blank_credentials_on_update_keep_the_stored_value()
    {
        var h = new SmsHarness();
        var created = await Create(h).Handle(new CreateSmsGatewayCommand(NewDto()), default);

        await Update(h).Handle(new UpdateSmsGatewayCommand(created.Id, new UpdateSmsGatewayDto
        {
            Name = "Renamed", Credentials = new() { ["api_key"] = "" },
        }), default);

        var stored = await h.Db.Set<SmsGateway>().SingleAsync();
        stored.Name.Should().Be("Renamed");
        h.Protector.Unprotect(stored.CredentialsCipher)["api_key"].Should().Be("plain-secret-1");
    }

    [Fact]
    public async Task Activating_moves_the_default_and_only_one_stays_active()
    {
        var h = new SmsHarness();
        var first = await Create(h).Handle(new CreateSmsGatewayCommand(NewDto("One")), default);
        var second = await Create(h).Handle(new CreateSmsGatewayCommand(NewDto("Two")), default);
        second.IsDefault.Should().BeFalse();

        await new ActivateSmsGatewayCommandHandler(h.Db, h.Runtime, NullLogger<ActivateSmsGatewayCommandHandler>.Instance)
            .Handle(new ActivateSmsGatewayCommand(second.Id), default);

        var gateways = await h.Db.Set<SmsGateway>().ToListAsync();
        gateways.Single(g => g.IsDefault).Id.Should().Be(second.Id);
        gateways.Single(g => g.Id == first.Id).IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task The_active_gateway_cannot_be_disabled_or_removed()
    {
        var h = new SmsHarness();
        var active = await Create(h).Handle(new CreateSmsGatewayCommand(NewDto()), default);

        var disable = () => new SetSmsGatewayEnabledCommandHandler(h.Db, h.Runtime).Handle(new SetSmsGatewayEnabledCommand(active.Id, false), default);
        var delete = () => new DeleteSmsGatewayCommandHandler(h.Db, NullLogger<DeleteSmsGatewayCommandHandler>.Instance).Handle(new DeleteSmsGatewayCommand(active.Id), default);

        await disable.Should().ThrowAsync<AppException>();
        await delete.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Balance_sync_stores_the_providers_balance()
    {
        var h = new SmsHarness();
        h.AddGateway();
        h.Fake.Balance = new SmsBalanceResult(true, true, 1234.5m, "BDT");

        var result = await new SyncSmsBalanceCommandHandler(h.Db, h.Runtime).Handle(new SyncSmsBalanceCommand(null, Force: true), default);

        result!.Balance.Should().Be(1234.5m);
        result.SyncedAt.Should().NotBeNull();
        (await h.Db.Set<SmsGateway>().SingleAsync()).LastBalance.Should().Be(1234.5m);
    }
}

public class SmsProviderParsingTests
{
    private static SmsProviderContext Context(HttpMessageHandler handler, Dictionary<string, string>? settings = null) => new()
    {
        GatewayName = "Test",
        BaseUrl = "https://vendor.test/api",
        SenderId = "uOrgHub",
        Secrets = new Dictionary<string, string> { ["api_key"] = "KEY-SECRET-99" },
        Settings = settings ?? new(),
        Http = new HttpClient(handler),
        Timeout = TimeSpan.FromSeconds(5),
    };

    private static readonly SmsOutgoingMessage Msg = new("8801712345678", "Hello", SmsEncoding.Gsm7, 1);

    [Fact]
    public async Task BulkSmsBd_accepts_response_code_202()
    {
        var http = StubHttpHandler.Json("""{"response_code":202,"message_id":"7781","success_message":"SMS Submitted Successfully"}""");

        var result = await new BulkSmsBdProvider().SendAsync(Context(http), Msg, default);

        result.Outcome.Should().Be(SmsSendOutcome.Accepted);
        result.ProviderMessageId.Should().Be("7781");
        http.Requests.Single().Body.Should().Contain("number=8801712345678").And.Contain("senderid=uOrgHub");
    }

    [Fact]
    public async Task BulkSmsBd_maps_insufficient_balance_to_a_rejection()
    {
        var http = StubHttpHandler.Json("""{"response_code":1007,"error_message":"Balance Insufficient"}""");

        var result = await new BulkSmsBdProvider().SendAsync(Context(http), Msg, default);

        result.Outcome.Should().Be(SmsSendOutcome.Rejected);
        result.ErrorCode.Should().Be("1007");
    }

    [Fact]
    public async Task AlphaSms_reads_the_request_id_and_balance()
    {
        var send = await new AlphaSmsProvider().SendAsync(
            Context(StubHttpHandler.Json("""{"error":0,"msg":"Request successfully submitted","data":{"request_id":55012}}""")), Msg, default);
        var balance = await new AlphaSmsProvider().GetBalanceAsync(
            Context(StubHttpHandler.Json("""{"error":0,"msg":"Success","data":{"balance":"5000.2500"}}""")), default);

        send.Outcome.Should().Be(SmsSendOutcome.Accepted);
        send.ProviderMessageId.Should().Be("55012");
        balance.Balance.Should().Be(5000.25m);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, SmsSendOutcome.Retryable)]
    [InlineData(HttpStatusCode.ServiceUnavailable, SmsSendOutcome.Retryable)]
    [InlineData(HttpStatusCode.Unauthorized, SmsSendOutcome.Rejected)]
    public async Task Http_errors_are_classified_by_whether_the_vendor_processed_them(HttpStatusCode status, SmsSendOutcome expected)
    {
        var result = await new AlphaSmsProvider().SendAsync(Context(StubHttpHandler.Json("{}", status)), Msg, default);
        result.Outcome.Should().Be(expected);
    }

    [Fact]
    public async Task Generic_http_fills_templates_and_reads_the_reply_by_path()
    {
        var http = StubHttpHandler.Json("""{"status":"OK","data":{"id":"G-1"}}""");
        var settings = new Dictionary<string, string>
        {
            ["send_url"] = "https://vendor.test/send?key={api_key}&to={to}&text={message}",
            ["phone_format"] = "01XXXXXXXXX",
            ["success_path"] = "status",
            ["success_value"] = "ok",
            ["message_id_path"] = "data.id",
        };

        var result = await new GenericHttpSmsProvider().SendAsync(Context(http, settings), Msg with { Text = "Hi & bye" }, default);

        result.Outcome.Should().Be(SmsSendOutcome.Accepted);
        result.ProviderMessageId.Should().Be("G-1");
        var url = http.Requests.Single().Request.RequestUri!.AbsoluteUri;
        url.Should().Contain("to=01712345678").And.Contain("text=Hi%20%26%20bye");
    }

    [Fact]
    public async Task Stored_responses_never_echo_the_api_key()
    {
        var http = StubHttpHandler.Json("""{"error":5,"msg":"Invalid key KEY-SECRET-99"}""");

        var result = await new AlphaSmsProvider().SendAsync(Context(http), Msg, default);

        result.RawResponse.Should().NotContain("KEY-SECRET-99").And.Contain("***");
    }
}

public class SmsAnalyticsTests
{
    [Fact]
    public async Task Buckets_by_local_day_and_counts_successes_and_failures()
    {
        var h = new SmsHarness();
        var gateway = h.AddGateway();
        // 19:00 UTC on the 1st is 01:00 on the 2nd in Bangladesh (UTC+6).
        AddMessage(h, gateway, new DateTime(2026, 11, 1, 19, 0, 0), SmsMessageStatus.Submitted, 2, 0.5m);
        AddMessage(h, gateway, new DateTime(2026, 11, 1, 10, 0, 0), SmsMessageStatus.Failed, 1, null);
        AddMessage(h, gateway, new DateTime(2026, 11, 3, 10, 0, 0), SmsMessageStatus.Delivered, 1, 0.25m);
        await h.Db.SaveChangesAsync();

        var result = await new GetSmsAnalyticsQueryHandler(h.Db).Handle(new GetSmsAnalyticsQuery(
            new DateTime(2026, 11, 1), new DateTime(2026, 11, 3), SmsAnalyticsGranularity.Day, 360, null), default);

        result.Points.Select(p => p.Period).Should().Equal("2026-11-01", "2026-11-02", "2026-11-03");
        result.Points[0].Failed.Should().Be(1);
        result.Points[1].Successful.Should().Be(1);
        result.Points[1].Segments.Should().Be(2);
        result.Totals.Sent.Should().Be(3);
        result.Totals.Successful.Should().Be(2);
        result.Totals.Cost.Should().Be(0.75m);
    }

    [Fact]
    public void Weeks_start_on_monday_and_months_on_the_first()
    {
        var thursday = new DateTime(2026, 11, 19);
        GetSmsAnalyticsQueryHandler.BucketStart(thursday, SmsAnalyticsGranularity.Week).Should().Be(new DateTime(2026, 11, 16));
        GetSmsAnalyticsQueryHandler.BucketStart(thursday, SmsAnalyticsGranularity.Month).Should().Be(new DateTime(2026, 11, 1));
    }

    private static void AddMessage(SmsHarness h, SmsGateway g, DateTime createdUtc, SmsMessageStatus status, int segments, decimal? cost)
        => h.Db.Set<SmsMessage>().Add(new SmsMessage
        {
            GatewayId = g.Id, GatewayName = g.Name, ProviderKey = g.ProviderKey, Recipient = "8801712345678",
            MessageText = "x", Segments = segments, Status = status, Cost = cost, CreatedAt = createdUtc,
        });
}

/// <summary>
/// Regression: BulkSMSBD's real error for a bad key is "user id not found in this {api_key} key".
/// Found against the live API; the key must not reach the stored test result or the response.
/// </summary>
public class SmsCredentialEchoTests
{
    [Fact]
    public async Task A_vendor_error_quoting_the_api_key_is_redacted_everywhere()
    {
        var h = new SmsHarness();
        var gateway = h.AddGateway();
        h.Fake.Balance = SmsBalanceResult.Failed("user id not found in this secret-key-123 key");

        var result = await new TestSmsGatewayCommandHandler(h.Db, h.Runtime, h.SendService())
            .Handle(new TestSmsGatewayCommand(gateway.Id, new TestSmsGatewayDto(), null), default);

        result.Success.Should().BeFalse();
        result.Message.Should().NotContain("secret-key-123").And.Contain("***");
        (await h.Db.Set<SmsGateway>().SingleAsync()).LastTestMessage.Should().NotContain("secret-key-123");
    }

    [Fact]
    public async Task A_send_error_quoting_the_api_key_is_redacted_before_storage()
    {
        var h = new SmsHarness();
        h.AddGateway();
        var (request, _) = await h.SendService().CreateAsync(["01712345678"], "Hi", "key-echo-01", "Manual", null);
        h.Fake.Script.Enqueue(_ => new SmsSendResult(SmsSendOutcome.Rejected, ErrorCode: "1011", ErrorMessage: "bad key secret-key-123"));

        await h.Dispatcher().ProcessQueuedAsync(10, default);

        var message = await h.Db.Set<SmsMessage>().SingleAsync(m => m.SendRequestId == request.Id);
        message.ErrorMessage.Should().NotContain("secret-key-123");
    }
}
