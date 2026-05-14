using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsModuleEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acc_cost_centers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_cost_centers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_cost_centers_acc_cost_centers_ParentCostCenterId",
                        column: x => x.ParentCostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_budgets_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_budgets_acc_fiscalyears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "acc_fiscalyears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_bank_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BranchName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RoutingNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ChartOfAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_bank_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_bank_accounts_acc_chartofaccounts_ChartOfAccountId",
                        column: x => x.ChartOfAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentTermsDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivableAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_customers_acc_chartofaccounts_ReceivableAccountId",
                        column: x => x.ReceivableAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_tax_rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaxType = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TaxAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_tax_rates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_tax_rates_acc_chartofaccounts_TaxAccountId",
                        column: x => x.TaxAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PaymentTermsDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PayableAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_vendors_acc_chartofaccounts_PayableAccountId",
                        column: x => x.PayableAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_budget_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Period = table.Column<int>(type: "integer", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_budget_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_budget_lines_acc_budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "acc_budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_acc_budget_lines_acc_chartofaccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_budget_lines_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_bank_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChequeNumber = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Payee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsReconciled = table.Column<bool>(type: "boolean", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_bank_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_bank_transactions_acc_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "acc_bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_bank_transactions_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_invoices_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_invoices_acc_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "acc_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_invoices_acc_fiscalyears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "acc_fiscalyears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_invoices_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VendorBillNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_bills_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_bills_acc_fiscalyears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "acc_fiscalyears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_bills_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_bills_acc_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "acc_vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChequeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: true),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    FiscalYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_payments_acc_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "acc_bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_payments_acc_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "acc_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_payments_acc_fiscalyears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "acc_fiscalyears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_payments_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_payments_acc_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "acc_vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_invoice_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineOrder = table.Column<int>(type: "integer", nullable: false),
                    TaxRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevenueAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_invoice_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_invoice_lines_acc_chartofaccounts_RevenueAccountId",
                        column: x => x.RevenueAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_invoice_lines_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_invoice_lines_acc_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "acc_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_acc_invoice_lines_acc_tax_rates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "acc_tax_rates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_bill_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineOrder = table.Column<int>(type: "integer", nullable: false),
                    TaxRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpenseAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_bill_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_bill_lines_acc_bills_BillId",
                        column: x => x.BillId,
                        principalTable: "acc_bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_acc_bill_lines_acc_chartofaccounts_ExpenseAccountId",
                        column: x => x.ExpenseAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_bill_lines_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_bill_lines_acc_tax_rates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "acc_tax_rates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_payment_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_payment_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_payment_allocations_acc_bills_BillId",
                        column: x => x.BillId,
                        principalTable: "acc_bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_payment_allocations_acc_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "acc_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_payment_allocations_acc_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "acc_payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acc_bank_accounts_AccountNumber",
                table: "acc_bank_accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_bank_accounts_ChartOfAccountId",
                table: "acc_bank_accounts",
                column: "ChartOfAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bank_transactions_BankAccountId",
                table: "acc_bank_transactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bank_transactions_JournalEntryId",
                table: "acc_bank_transactions",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bill_lines_BillId",
                table: "acc_bill_lines",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bill_lines_CostCenterId",
                table: "acc_bill_lines",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bill_lines_ExpenseAccountId",
                table: "acc_bill_lines",
                column: "ExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bill_lines_TaxRateId",
                table: "acc_bill_lines",
                column: "TaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bills_BillNumber",
                table: "acc_bills",
                column: "BillNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_bills_CostCenterId",
                table: "acc_bills",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bills_FiscalYearId",
                table: "acc_bills",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bills_JournalEntryId",
                table: "acc_bills",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bills_VendorId",
                table: "acc_bills",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_budget_lines_AccountId",
                table: "acc_budget_lines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_budget_lines_BudgetId",
                table: "acc_budget_lines",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_budget_lines_CostCenterId",
                table: "acc_budget_lines",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_budgets_CostCenterId",
                table: "acc_budgets",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_budgets_FiscalYearId",
                table: "acc_budgets",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_cost_centers_Code",
                table: "acc_cost_centers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_cost_centers_ParentCostCenterId",
                table: "acc_cost_centers",
                column: "ParentCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_customers_CustomerCode",
                table: "acc_customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_customers_ReceivableAccountId",
                table: "acc_customers",
                column: "ReceivableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoice_lines_CostCenterId",
                table: "acc_invoice_lines",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoice_lines_InvoiceId",
                table: "acc_invoice_lines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoice_lines_RevenueAccountId",
                table: "acc_invoice_lines",
                column: "RevenueAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoice_lines_TaxRateId",
                table: "acc_invoice_lines",
                column: "TaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoices_CostCenterId",
                table: "acc_invoices",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoices_CustomerId",
                table: "acc_invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoices_FiscalYearId",
                table: "acc_invoices",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoices_InvoiceNumber",
                table: "acc_invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoices_JournalEntryId",
                table: "acc_invoices",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payment_allocations_BillId",
                table: "acc_payment_allocations",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payment_allocations_InvoiceId",
                table: "acc_payment_allocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payment_allocations_PaymentId",
                table: "acc_payment_allocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_BankAccountId",
                table: "acc_payments",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_CustomerId",
                table: "acc_payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_FiscalYearId",
                table: "acc_payments",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_JournalEntryId",
                table: "acc_payments",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_PaymentNumber",
                table: "acc_payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_VendorId",
                table: "acc_payments",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_tax_rates_Code",
                table: "acc_tax_rates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_tax_rates_TaxAccountId",
                table: "acc_tax_rates",
                column: "TaxAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vendors_PayableAccountId",
                table: "acc_vendors",
                column: "PayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vendors_VendorCode",
                table: "acc_vendors",
                column: "VendorCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acc_bank_transactions");

            migrationBuilder.DropTable(
                name: "acc_bill_lines");

            migrationBuilder.DropTable(
                name: "acc_budget_lines");

            migrationBuilder.DropTable(
                name: "acc_invoice_lines");

            migrationBuilder.DropTable(
                name: "acc_payment_allocations");

            migrationBuilder.DropTable(
                name: "acc_budgets");

            migrationBuilder.DropTable(
                name: "acc_tax_rates");

            migrationBuilder.DropTable(
                name: "acc_bills");

            migrationBuilder.DropTable(
                name: "acc_invoices");

            migrationBuilder.DropTable(
                name: "acc_payments");

            migrationBuilder.DropTable(
                name: "acc_cost_centers");

            migrationBuilder.DropTable(
                name: "acc_bank_accounts");

            migrationBuilder.DropTable(
                name: "acc_customers");

            migrationBuilder.DropTable(
                name: "acc_vendors");
        }
    }
}
