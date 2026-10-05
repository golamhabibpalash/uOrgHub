using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Features.Payment;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.Clients.Commands;
using uOrgHub.Projects.Features.RABills.Commands;
using uOrgHub.Projects.Features.RABills.Queries;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Tests.Projects;

/// <summary>
/// The Mr. A scenario end to end: client → contract (8,000,000) → RA bill #1 (2,000,000 gross, 5%
/// retention) → posted AR invoice (1,900,000) → partial receipt (1,000,000) → outstanding 900,000.
/// Accounts' real create/post/payment handlers run underneath, so the ledger effect is asserted too.
/// </summary>
public class ClientBillingChainTests : IDisposable
{
    private readonly AppDbContext _ctx = TestDb.NewContext("TestDb_ClientBilling_" + Guid.NewGuid());
    private readonly JournalEntryService _jeService;
    private readonly JournalEntryRepository _jeRepo;
    private readonly IDocumentNumberingService _numbering;
    private readonly ISender _sender;

    private readonly ChartOfAccount _receivable, _revenue, _bank;
    private readonly Client _client;
    private readonly Project _project;
    private readonly Guid _costCenterId = Guid.NewGuid();

    public ClientBillingChainTests()
    {
        _jeRepo = new JournalEntryRepository(_ctx);
        _jeService = new JournalEntryService(_ctx, _jeRepo, new CreateJournalEntryValidator(),
            new UpdateJournalEntryValidator(), new JournalEntrySourceService(_ctx));
        var seq = 0;
        var numbering = new Mock<IDocumentNumberingService>();
        numbering.Setup(x => x.GenerateNextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync((string _, string prefix, int? _, int? _) => $"{prefix}-2026-{++seq:0000}");
        _numbering = numbering.Object;
        _sender = new AccountsSender(_ctx, _numbering, _jeService, _jeRepo);

        _receivable = Account("1200", "Accounts Receivable", AccountGroupType.Asset);
        _revenue = Account("4100", "Construction Revenue", AccountGroupType.Income);
        _bank = Account("1100", "Bank", AccountGroupType.Asset);
        _ctx.Set<FiscalYear>().Add(new FiscalYear
        {
            Id = Guid.NewGuid(), Name = "FY 2026-27", Status = FiscalYearStatus.Active,
            StartDate = new DateTime(2026, 7, 1), EndDate = new DateTime(2027, 6, 30)
        });
        _ctx.Set<BankAccount>().Add(new BankAccount
        {
            Id = Guid.NewGuid(), AccountNumber = "001", AccountName = "Operating", BankName = "Bank", ChartOfAccountId = _bank.Id
        });

        _client = new Client { Id = Guid.NewGuid(), ClientCode = "CLT-0001", CompanyName = "Mr. A", Phone = "01700000000", Address = "Dhaka" };
        _project = new Project
        {
            Id = Guid.NewGuid(), ProjectCode = "PRJ-001", ProjectName = "Mr. A Residence", ClientId = _client.Id,
            CategoryId = Guid.NewGuid(), ProjectManagerId = Guid.NewGuid(),
            StartDate = new DateTime(2026, 7, 1), PlannedEndDate = new DateTime(2027, 6, 30), ContractValue = 8_000_000m
        };
        _ctx.Set<Client>().Add(_client);
        _ctx.Set<Project>().Add(_project);
        _ctx.Set<CostCenter>().Add(new CostCenter { Id = _costCenterId, Code = "PRJ-001", Name = "Mr. A Residence", ProjectId = _project.Id, IsActive = true });
        _ctx.SaveChanges();
    }

    public void Dispose() => _ctx.Dispose();

    private ChartOfAccount Account(string code, string name, AccountGroupType type)
    {
        var a = new ChartOfAccount { Id = Guid.NewGuid(), AccountCode = code, AccountName = name, AccountType = type, IsActive = true, AllowDirectEntry = true };
        _ctx.Set<ChartOfAccount>().Add(a);
        _ctx.SaveChanges();
        return a;
    }

    private async Task LinkCustomerAsync()
        => await new CreateCustomerFromClientCommandHandler(_ctx, _sender).Handle(
            new CreateCustomerFromClientCommand(_client.Id, new CreateCustomerFromClientDto { ReceivableAccountId = _receivable.Id }), default);

    /// <summary>RA bill #1 certified at 2,000,000 gross with 5% retention → net 1,900,000.</summary>
    private async Task<RABill> CertifiedBillAsync()
    {
        var bill = new RABill
        {
            Id = Guid.NewGuid(), ProjectId = _project.Id, BillNumber = "RAB-001", Title = "Foundation & ground floor",
            BillDate = new DateTime(2026, 9, 1), PeriodFrom = new DateTime(2026, 7, 1), PeriodTo = new DateTime(2026, 8, 31),
            BillSequence = 1, SubmittedById = Guid.NewGuid(), RetentionPercent = 5, Status = RABillStatus.Submitted
        };
        _ctx.Set<RABill>().Add(bill);
        _ctx.SaveChanges();
        await new CertifyRABillCommandHandler(_ctx).Handle(new CertifyRABillCommand(bill.Id, new CertifyRABillDto
        {
            GrossAmount = 2_000_000m, CertifiedById = Guid.NewGuid(), CertifiedDate = new DateTime(2026, 9, 5)
        }), default);
        return bill;
    }

    private Task<RABillResponseDto> RaiseAsync(Guid billId)
        => new RaiseRABillInvoiceCommandHandler(_ctx, _sender).Handle(
            new RaiseRABillInvoiceCommand(billId, new RaiseRABillInvoiceDto { RevenueAccountId = _revenue.Id }), default);

    private async Task ReceiveAsync(Guid invoiceId, decimal amount)
    {
        var customerId = _ctx.Set<Client>().Single(c => c.Id == _client.Id).CustomerId!.Value;
        await new CreatePaymentCommandHandler(_ctx, _numbering, _jeService, _jeRepo).Handle(new CreatePaymentCommand(new CreatePaymentDto
        {
            PaymentType = PaymentType.CustomerPayment, PaymentMethod = PaymentMethod.BankTransfer,
            PaymentDate = new DateTime(2026, 9, 20), Amount = amount, FiscalYearId = _ctx.Set<FiscalYear>().First().Id,
            CustomerId = customerId, BankAccountId = _ctx.Set<BankAccount>().First().Id,
            Allocations = [new CreatePaymentAllocationDto { InvoiceId = invoiceId, AllocatedAmount = amount }]
        }), default);
    }

    private Task<ContractAccountDto> ContractAccountAsync()
        => new GetContractAccountQueryHandler(_ctx).Handle(new GetContractAccountQuery(_project.Id), default);

    [Fact]
    public async Task Client_gets_an_AR_customer_created_from_its_own_details()
    {
        await LinkCustomerAsync();

        var client = _ctx.Set<Client>().Include(c => c.Customer).Single(c => c.Id == _client.Id);
        client.Customer!.Name.Should().Be("Mr. A");
        client.Customer.Phone.Should().Be("01700000000");
        client.Customer.ReceivableAccountId.Should().Be(_receivable.Id);
    }

    [Fact]
    public async Task Certified_RA_bill_raises_a_posted_invoice_for_its_net_amount_on_the_project()
    {
        await LinkCustomerAsync();
        var bill = await CertifiedBillAsync();

        var result = await RaiseAsync(bill.Id);

        result.InvoiceNumber.Should().NotBeNullOrEmpty();
        result.InvoiceTotal.Should().Be(1_900_000m);
        result.PaymentState.Should().Be(RABillPaymentStates.Unpaid);

        var invoice = _ctx.Set<Invoice>().Include(i => i.JournalEntry!).ThenInclude(j => j.Lines).Single();
        invoice.Status.Should().Be(InvoiceStatus.Sent, "it is posted, not left as a draft");
        invoice.CostCenterId.Should().Be(_costCenterId);
        invoice.JournalEntry!.Lines.Single(l => l.DebitAmount > 0).AccountId.Should().Be(_receivable.Id);
        var revenueLine = invoice.JournalEntry.Lines.Single(l => l.CreditAmount > 0);
        revenueLine.AccountId.Should().Be(_revenue.Id);
        revenueLine.CreditAmount.Should().Be(1_900_000m);
        revenueLine.CostCenterId.Should().Be(_costCenterId);
        _ctx.Set<Project>().Single().DefaultRevenueAccountId.Should().Be(_revenue.Id);
    }

    [Fact]
    public async Task Mr_A_partial_payment_leaves_900k_outstanding_with_retention_held()
    {
        await LinkCustomerAsync();
        var bill = await CertifiedBillAsync();
        var raised = await RaiseAsync(bill.Id);

        await ReceiveAsync(raised.InvoiceId!.Value, 1_000_000m);
        var account = await ContractAccountAsync();

        account.ContractValue.Should().Be(8_000_000m);
        account.CertifiedGross.Should().Be(2_000_000m);
        account.RemainingToBill.Should().Be(6_000_000m);
        account.Invoiced.Should().Be(1_900_000m);
        account.Received.Should().Be(1_000_000m);
        account.Outstanding.Should().Be(900_000m);
        account.RetentionHeld.Should().Be(100_000m);
        account.RetentionOutstanding.Should().Be(100_000m);
        account.NotYetInvoiced.Should().Be(0m);
        account.Bills.Single().PaymentState.Should().Be(RABillPaymentStates.PartiallyPaid);
        account.Bills.Single().InvoiceBalance.Should().Be(900_000m);

        await ReceiveAsync(raised.InvoiceId!.Value, 900_000m);
        (await ContractAccountAsync()).Bills.Single().PaymentState.Should().Be(RABillPaymentStates.Paid);
    }

    [Fact]
    public async Task Raising_is_refused_without_a_customer_link()
    {
        var bill = await CertifiedBillAsync();

        var act = () => RaiseAsync(bill.Id);

        await act.Should().ThrowAsync<AppException>().WithMessage("*not linked to an Accounts customer*");
        _ctx.Set<Invoice>().Should().BeEmpty();
    }

    [Fact]
    public async Task A_bill_is_invoiced_only_once_and_cannot_then_be_marked_paid_by_hand()
    {
        await LinkCustomerAsync();
        var bill = await CertifiedBillAsync();
        await RaiseAsync(bill.Id);

        var again = () => RaiseAsync(bill.Id);
        var markPaid = () => new MarkRABillPaidCommandHandler(_ctx).Handle(new MarkRABillPaidCommand(bill.Id), default);

        await again.Should().ThrowAsync<AppException>().WithMessage("*already invoiced*");
        await markPaid.Should().ThrowAsync<AppException>().WithMessage("*Record the client's payment*");
    }

    [Fact]
    public async Task Retention_release_is_capped_at_what_is_still_held()
    {
        await LinkCustomerAsync();
        var bill = await CertifiedBillAsync();
        await RaiseAsync(bill.Id);
        var handler = new RaiseRetentionInvoiceCommandHandler(_ctx, _sender);
        RaiseRetentionInvoiceDto Release(decimal amount) => new()
        {
            ProjectId = _project.Id, Amount = amount, RevenueAccountId = _revenue.Id, ReleaseDate = new DateTime(2026, 12, 1)
        };

        var tooMuch = () => handler.Handle(new RaiseRetentionInvoiceCommand(Release(150_000m)), default);
        await tooMuch.Should().ThrowAsync<AppException>().WithMessage("*100,000.00*");

        await handler.Handle(new RaiseRetentionInvoiceCommand(Release(100_000m)), default);
        var account = await ContractAccountAsync();
        account.RetentionReleased.Should().Be(100_000m);
        account.RetentionOutstanding.Should().Be(0m);
        account.Invoiced.Should().Be(2_000_000m, "1,900,000 RA invoice + 100,000 retention release");
    }
}

/// <summary>Routes the Accounts commands the Projects handlers send to Accounts' real handlers.</summary>
public class AccountsSender(AppDbContext ctx, IDocumentNumberingService numbering, IJournalEntryService je, IJournalEntryRepository jeRepo) : ISender
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        object result = request switch
        {
            CreateCustomerCommand c => await new CreateCustomerCommandHandler(ctx).Handle(c, ct),
            CreateInvoiceCommand c => await new CreateInvoiceCommandHandler(ctx, numbering).Handle(c, ct),
            PostInvoiceCommand c => await new PostInvoiceCommandHandler(ctx, je, jeRepo).Handle(c, ct),
            _ => throw new NotSupportedException(request.GetType().Name)
        };
        return (TResponse)result;
    }

    public Task Send<TRequest>(TRequest request, CancellationToken ct = default) where TRequest : IRequest => throw new NotSupportedException();
    public Task<object?> Send(object request, CancellationToken ct = default) => throw new NotSupportedException();
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken ct = default) => throw new NotSupportedException();
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken ct = default) => throw new NotSupportedException();
}
