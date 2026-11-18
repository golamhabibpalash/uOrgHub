using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.PartySms;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Features.PartySms;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models.Enums;
using uOrgHub.Shared.Services;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Templates.Commands;
using uOrgHub.Sms.Features.Templates.Queries;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Services;

namespace uOrgHub.Tests.Sms;

public sealed class FakePartyDirectory : IPartyContactDirectory
{
    public Dictionary<(PartyType, Guid), PartyContact> Parties { get; } = new();

    public PartyContact Add(PartyType type, string name, string? phone)
    {
        var contact = new PartyContact(type, Guid.NewGuid(), name, phone);
        Parties[(type, contact.Id)] = contact;
        return contact;
    }

    public Task<PartyContact?> FindAsync(PartyType type, Guid id, CancellationToken ct = default)
        => Task.FromResult(Parties.GetValueOrDefault((type, id)));
}

public class SmsTemplateTests
{
    [Fact]
    public async Task Each_event_falls_back_to_its_default_until_customised()
    {
        var h = new SmsHarness();

        var templates = await new GetSmsTemplatesQueryHandler(h.Db).Handle(new GetSmsTemplatesQuery(), default);

        templates.Select(t => t.EventKey).Should().Equal(SmsTemplateEvents.MoneyReceived, SmsTemplateEvents.MoneyPaid);
        templates.Should().OnlyContain(t => !t.IsCustom && t.Body == t.DefaultBody);
    }

    [Fact]
    public async Task Saving_and_resetting_a_template()
    {
        var h = new SmsHarness();
        var update = new UpdateSmsTemplateCommandHandler(h.Db);

        var saved = await update.Handle(new UpdateSmsTemplateCommand("moneyreceived", new UpdateSmsTemplateDto { Body = "Got {amount} from {party}." }), default);
        saved.IsCustom.Should().BeTrue();

        var rendered = await new SmsTemplateService(h.Db, new FixedCompany(null)).RenderAsync(SmsTemplateEvents.MoneyReceived,
            new Dictionary<string, string> { ["amount"] = "500.00", ["party"] = "Rahim" });
        rendered.Should().Be("Got 500.00 from Rahim.");

        var reset = await new ResetSmsTemplateCommandHandler(h.Db).Handle(new ResetSmsTemplateCommand(SmsTemplateEvents.MoneyReceived), default);
        reset.IsCustom.Should().BeFalse();
        (await h.Db.Set<SmsTemplate>().CountAsync(t => !t.IsDeleted)).Should().Be(0);
    }

    /// <summary>A typo would otherwise go out literally as "{amout}".</summary>
    [Fact]
    public async Task Unknown_placeholders_are_refused()
    {
        var h = new SmsHarness();

        var act = () => new UpdateSmsTemplateCommandHandler(h.Db).Handle(
            new UpdateSmsTemplateCommand(SmsTemplateEvents.MoneyPaid, new UpdateSmsTemplateDto { Body = "Paid {amout}" }), default);

        (await act.Should().ThrowAsync<ValidationException>()).Which.Errors.Single().Should().Contain("{amout}");
    }

    [Fact]
    public async Task Company_name_is_filled_in_from_the_current_company()
    {
        var h = new SmsHarness();
        var company = new uOrgHub.Shared.Entities.Company { Name = "Circle Facade Ltd" };
        h.Db.Set<uOrgHub.Shared.Entities.Company>().Add(company);
        await h.Db.SaveChangesAsync();

        var text = await new SmsTemplateService(h.Db, new FixedCompany(company.Id))
            .RenderAsync(SmsTemplateEvents.MoneyPaid, new Dictionary<string, string> { ["party"] = "X", ["amount"] = "1.00", ["date"] = "d", ["reference"] = "r" });

        text.Should().EndWith("- Circle Facade Ltd");
    }
}

public class PartySmsTests
{
    private sealed class Ctx
    {
        public SmsHarness H { get; } = new();
        public FakePartyDirectory Parties { get; } = new();
        public SmsTemplateService Templates => new(H.Db, new FixedCompany(null));
        public SmsSendService Sender => H.SendService();

        public Ctx() => H.AddGateway();

        public Voucher Voucher(VoucherType type, VoucherStatus status, PartyContact? party)
        {
            var cash = new ChartOfAccount { AccountCode = "1400", AccountName = "Cash in Hand", AccountType = AccountGroupType.Asset, AccountGroupId = Guid.NewGuid() };
            var other = new ChartOfAccount { AccountCode = "5000", AccountName = "Site Materials", AccountType = AccountGroupType.Expense, AccountGroupId = Guid.NewGuid() };
            var v = new Voucher
            {
                VoucherNumber = "DR-0001", VoucherType = type, VoucherDate = new DateTime(2026, 11, 18), Description = "x",
                DebitAccount = type == VoucherType.Credit ? cash : other, CreditAccount = type == VoucherType.Credit ? other : cash,
                Amount = 25_000m, Status = status, PartyType = party?.Type, PartyId = party?.Id,
            };
            H.Db.Set<Voucher>().Add(v);
            H.Db.SaveChanges();
            return v;
        }
    }

    [Fact]
    public async Task Posted_debit_voucher_previews_the_money_paid_template_to_the_partys_number()
    {
        var c = new Ctx();
        var vendor = c.Parties.Add(PartyType.Vendor, "Rahim Traders", "01712-345678");
        var v = c.Voucher(VoucherType.Debit, VoucherStatus.Posted, vendor);

        var preview = await new GetVoucherSmsPreviewQueryHandler(c.H.Db, c.Parties, c.Sender, c.Templates)
            .Handle(new GetVoucherSmsPreviewQuery(v.Id), default);

        preview.CanSend.Should().BeTrue();
        preview.Phone.Should().Be("8801712345678");
        preview.EventKey.Should().Be(SmsTemplateEvents.MoneyPaid);
        preview.Message.Should().Contain("Rahim Traders").And.Contain("25,000.00").And.Contain("18/11/2026").And.Contain("DR-0001");
    }

    [Fact]
    public async Task Contra_vouchers_have_no_sms()
    {
        var c = new Ctx();
        var v = c.Voucher(VoucherType.Contra, VoucherStatus.Posted, c.Parties.Add(PartyType.Customer, "Karim", "01812345678"));

        var preview = await new GetVoucherSmsPreviewQueryHandler(c.H.Db, c.Parties, c.Sender, c.Templates)
            .Handle(new GetVoucherSmsPreviewQuery(v.Id), default);

        preview.CanSend.Should().BeFalse();
        preview.Reason.Should().ContainEquivalentOf("contra");
    }

    /// <summary>The post confirmation shows the message before posting, so an approved voucher previews.</summary>
    [Fact]
    public async Task An_approved_voucher_previews_its_message_before_posting()
    {
        var c = new Ctx();
        var v = c.Voucher(VoucherType.Credit, VoucherStatus.Approved, c.Parties.Add(PartyType.Customer, "Karim", "01812345678"));

        var preview = await new GetVoucherSmsPreviewQueryHandler(c.H.Db, c.Parties, c.Sender, c.Templates)
            .Handle(new GetVoucherSmsPreviewQuery(v.Id), default);

        preview.CanSend.Should().BeTrue();
        preview.EventKey.Should().Be(SmsTemplateEvents.MoneyReceived);
        preview.Message.Should().Contain("received").And.Contain("Karim");
    }

    [Theory]
    [InlineData(null, "no phone")]
    [InlineData("12345", "not a valid mobile")]
    public async Task A_party_without_a_usable_number_is_explained(string? phone, string reason)
    {
        var c = new Ctx();
        var v = c.Voucher(VoucherType.Credit, VoucherStatus.Posted, c.Parties.Add(PartyType.Client, "Sagupta", phone));

        var preview = await new GetVoucherSmsPreviewQueryHandler(c.H.Db, c.Parties, c.Sender, c.Templates)
            .Handle(new GetVoucherSmsPreviewQuery(v.Id), default);

        preview.CanSend.Should().BeFalse();
        preview.Reason.Should().ContainEquivalentOf(reason);
        preview.Message.Should().Contain("Sagupta"); // still shown, so the user sees what would go out
    }

    [Fact]
    public async Task Sending_uses_the_record_number_the_edited_text_and_tags_the_voucher()
    {
        var c = new Ctx();
        var v = c.Voucher(VoucherType.Credit, VoucherStatus.Posted, c.Parties.Add(PartyType.Customer, "Karim", "+8801812345678"));
        var handler = new SendVoucherSmsCommandHandler(c.H.Db, c.Parties, c.Sender, c.Templates);
        var dto = new SendPartySmsDto { Message = "  Thanks Karim, received.  ", IdempotencyKey = "dialog-0001" };

        var first = await handler.Handle(new SendVoucherSmsCommand(v.Id, dto), default);
        var again = await handler.Handle(new SendVoucherSmsCommand(v.Id, dto), default);

        again.RequestId.Should().Be(first.RequestId); // double-click sends once
        var request = await c.H.Db.Set<SmsSendRequest>().Include(r => r.Messages).SingleAsync();
        request.Source.Should().Be("Voucher");
        request.SourceReference.Should().Be("DR-0001");
        request.Messages.Single().Recipient.Should().Be("8801812345678");
        request.Messages.Single().MessageText.Should().Be("Thanks Karim, received.");
    }

    [Fact]
    public async Task Sending_for_a_voucher_that_cannot_send_is_refused()
    {
        var c = new Ctx();
        var v = c.Voucher(VoucherType.Debit, VoucherStatus.Draft, c.Parties.Add(PartyType.Vendor, "V", "01712345678"));

        var act = () => new SendVoucherSmsCommandHandler(c.H.Db, c.Parties, c.Sender, c.Templates)
            .Handle(new SendVoucherSmsCommand(v.Id, new SendPartySmsDto { IdempotencyKey = "dialog-0002" }), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*posted*");
        (await c.H.Db.Set<SmsMessage>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Payment_received_uses_the_mr_number_and_method()
    {
        var c = new Ctx();
        var customer = c.Parties.Add(PartyType.Customer, "Karim", "01812345678");
        var payment = new Payment
        {
            PaymentNumber = "PMT-0009", MoneyReceiptNumber = "MR-00125", PaymentType = PaymentType.CustomerPayment,
            PaymentMethod = PaymentMethod.BankTransfer, PaymentDate = new DateTime(2026, 11, 18), Amount = 1500m,
            CustomerId = customer.Id, FiscalYearId = Guid.NewGuid(),
        };
        c.H.Db.Set<Payment>().Add(payment);
        await c.H.Db.SaveChangesAsync();
        await new UpdateSmsTemplateCommandHandler(c.H.Db).Handle(new UpdateSmsTemplateCommand(SmsTemplateEvents.MoneyReceived,
            new UpdateSmsTemplateDto { Body = "{party} paid {amount} by {method}, {reference}" }), default);

        var preview = await new GetPaymentSmsPreviewQueryHandler(c.H.Db, c.Parties, c.Sender, c.Templates)
            .Handle(new GetPaymentSmsPreviewQuery(payment.Id), default);

        preview.CanSend.Should().BeTrue();
        preview.Message.Should().Be("Karim paid 1,500.00 by Bank Transfer, MR-00125");
    }
}

public class VoucherPartyValidationTests
{
    private static CreateVoucherDto Dto() => new()
    {
        VoucherType = VoucherType.Credit, VoucherDate = DateTime.UtcNow, Description = "x",
        DebitAccountId = Guid.NewGuid(), CreditAccountId = Guid.NewGuid(), Amount = 10, CostCenterId = Guid.NewGuid(),
    };

    [Fact]
    public void Party_type_and_id_go_together()
    {
        var dto = Dto();
        dto.PartyType = PartyType.Customer;
        new CreateVoucherValidator().Validate(dto).Errors.Should().Contain(e => e.ErrorMessage.Contains("party type and the party"));
    }

    [Fact]
    public void Sms_on_post_needs_a_party_and_is_not_offered_for_contra()
    {
        var noParty = Dto();
        noParty.SendSmsOnPost = true;
        new CreateVoucherValidator().Validate(noParty).Errors.Should().Contain(e => e.ErrorMessage.Contains("Pick a customer"));

        var contra = Dto();
        contra.VoucherType = VoucherType.Contra;
        contra.PartyType = PartyType.Vendor;
        contra.PartyId = Guid.NewGuid();
        contra.SendSmsOnPost = true;
        new CreateVoucherValidator().Validate(contra).Errors.Should().Contain(e => e.ErrorMessage.Contains("contra"));
    }

    private static async Task<uOrgHub.Accounts.DTOs.Voucher.VoucherResponseDto> CreateAsync(FakePartyDirectory parties, CreateVoucherDto dto, uOrgHub.Shared.Data.AppDbContext db)
    {
        var numbering = new Moq.Mock<uOrgHub.Accounts.Services.IDocumentNumberingService>();
        numbering.Setup(x => x.GenerateNextAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<int?>()))
            .ReturnsAsync("DR-2026-0001");
        return await new uOrgHub.Accounts.Features.Voucher.CreateVoucherCommandHandler(db, numbering.Object, new CreateVoucherValidator(), parties)
            .Handle(new uOrgHub.Accounts.Features.Voucher.CreateVoucherCommand(dto, "rafiq"), default);
    }

    private static CreateVoucherDto SeededDebit(uOrgHub.Shared.Data.AppDbContext db)
    {
        var expense = new ChartOfAccount { AccountCode = "5001", AccountName = "Site Materials", AccountType = AccountGroupType.Expense, IsActive = true, AllowDirectEntry = true, AccountGroupId = Guid.NewGuid() };
        var bank = new ChartOfAccount { AccountCode = "1101", AccountName = "Bank", AccountType = AccountGroupType.Asset, IsActive = true, AllowDirectEntry = true, IsCashOrBank = true, AccountGroupId = Guid.NewGuid() };
        var cc = new CostCenter { Code = "CC-1", Name = "Head Office", IsActive = true };
        db.AddRange(expense, bank, cc);
        db.SaveChanges();
        return new CreateVoucherDto
        {
            VoucherType = VoucherType.Debit, VoucherDate = new DateTime(2026, 11, 18), Description = "Cement",
            DebitAccountId = expense.Id, CreditAccountId = bank.Id, CostCenterId = cc.Id, Amount = 500m,
        };
    }

    [Fact]
    public async Task Sms_on_post_for_a_party_without_a_phone_is_refused_at_save()
    {
        var db = TestDb.NewContext();
        var parties = new FakePartyDirectory();
        var party = parties.Add(PartyType.Vendor, "No Phone Ltd", null);
        var dto = SeededDebit(db);
        dto.PartyType = PartyType.Vendor;
        dto.PartyId = party.Id;
        dto.SendSmsOnPost = true;

        var act = () => CreateAsync(parties, dto, db);

        (await act.Should().ThrowAsync<ValidationException>()).Which.Errors.Single().Should().Contain("No Phone Ltd");
    }

    [Fact]
    public async Task Picking_a_party_fills_the_paid_to_name_when_left_blank()
    {
        var db = TestDb.NewContext();
        var parties = new FakePartyDirectory();
        var party = parties.Add(PartyType.Vendor, "Rahim Traders", "01712345678");
        var dto = SeededDebit(db);
        dto.PartyType = PartyType.Vendor;
        dto.PartyId = party.Id;
        dto.SendSmsOnPost = true;

        var created = await CreateAsync(parties, dto, db);

        created.Name.Should().Be("Rahim Traders");
        created.PartyId.Should().Be(party.Id);
        created.SendSmsOnPost.Should().BeTrue();
    }
}
