using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Sister-concern isolation, phase 1 (SISTER_CONCERN_PLAN.md) — scopes Accounts' financial
    /// documents (Bill, Voucher, Invoice, Payment, Budget, BankAccount, JournalEntry,
    /// NumberingSequence) to a CompanyId. Hand-edited after `dotnet ef migrations add`, same
    /// safety pattern as 20260917191746_UnifyVendorMaster: add each CompanyId column nullable
    /// first, backfill it with real data, THEN tighten to NOT NULL — never add a NOT NULL column
    /// with a Guid.Empty default onto a table that might already have rows, since nothing in
    /// `companies` has Id = Guid.Empty and the FK below would either fail to attach or (worse)
    /// silently reference a company that doesn't exist.
    ///
    /// Also backfills `auth_user_companies` for any existing user with no membership row yet —
    /// without one, that user carries no "company_id" JWT claim after this ships, and every
    /// company-scoped table they touch would look empty to them.
    /// </remarks>
    public partial class UnifySisterConcernAccountsPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Add every CompanyId column nullable first — see remarks above. ──────────────
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "auth_refresh_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_vouchers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_numbering_sequences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_journalentries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_budgets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_bills",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_bank_accounts",
                type: "uuid",
                nullable: true);

            // ── 2. Backfill every existing row to the earliest-created company. In practice
            // there is at most one company before this phase ships (multi-company installs don't
            // exist yet); if `companies` is empty, these tables are necessarily empty too — no
            // user or financial document can be created before the setup wizard creates the
            // first company — so every UPDATE below is a safe no-op in that case. ────────────────
            migrationBuilder.Sql(@"
                UPDATE acc_vouchers SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_payments SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_numbering_sequences SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_journalentries SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_invoices SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_budgets SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_bills SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_bank_accounts SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
            ");

            // ── 3. Give every existing user without a UserCompany row membership in that same
            // company, so they get a "company_id" claim the next time they log in or refresh. ──
            migrationBuilder.Sql(@"
                INSERT INTO auth_user_companies (
                    ""Id"", ""UserId"", ""CompanyId"", ""IsDefault"", ""RoleInCompany"",
                    ""AssignedAt"", ""AssignedBy"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted""
                )
                SELECT
                    gen_random_uuid(), u.""Id"", c.""Id"", true, 'Member',
                    (now() at time zone 'utc'), 'System', (now() at time zone 'utc'), 'System', false
                FROM auth_users u
                CROSS JOIN LATERAL (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) c
                WHERE u.""IsDeleted"" = false
                  AND NOT EXISTS (
                      SELECT 1 FROM auth_user_companies uc
                      WHERE uc.""UserId"" = u.""Id"" AND uc.""IsDeleted"" = false
                  );
            ");

            // ── 4. Now safe to tighten every backfilled column to NOT NULL. ────────────────────
            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_vouchers", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_payments", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_numbering_sequences", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_journalentries", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_invoices", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_budgets", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_bills", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_bank_accounts", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            // ── 5. Drop the old globally-unique number indexes, including the one raw-SQL index
            // (acc_journalentries.EntryNumber) that was never in the EF model to begin with — see
            // JournalEntryConfiguration.cs. Each is replaced by a (CompanyId, Number) composite
            // below: two sister concerns number documents independently (SISTER_CONCERN_PLAN.md
            // §7) and can legitimately collide on the bare number. ────────────────────────────────
            migrationBuilder.DropIndex(name: "IX_acc_vouchers_VoucherNumber", table: "acc_vouchers");
            migrationBuilder.DropIndex(name: "IX_acc_payments_PaymentNumber", table: "acc_payments");
            migrationBuilder.DropIndex(name: "IX_acc_numbering_sequences_DocumentType_Prefix_Year_Month", table: "acc_numbering_sequences");
            migrationBuilder.DropIndex(name: "IX_acc_invoices_InvoiceNumber", table: "acc_invoices");
            migrationBuilder.DropIndex(name: "IX_acc_bills_BillNumber", table: "acc_bills");
            migrationBuilder.DropIndex(name: "IX_acc_bank_accounts_AccountNumber", table: "acc_bank_accounts");
            migrationBuilder.DropIndex(name: "IX_acc_journalentries_EntryNumber", table: "acc_journalentries");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_CompanyId_VoucherNumber",
                table: "acc_vouchers",
                columns: new[] { "CompanyId", "VoucherNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_CompanyId_PaymentNumber",
                table: "acc_payments",
                columns: new[] { "CompanyId", "PaymentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_numbering_sequences_CompanyId_DocumentType_Prefix_Year_~",
                table: "acc_numbering_sequences",
                columns: new[] { "CompanyId", "DocumentType", "Prefix", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_journalentries_CompanyId_EntryNumber",
                table: "acc_journalentries",
                columns: new[] { "CompanyId", "EntryNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_invoices_CompanyId_InvoiceNumber",
                table: "acc_invoices",
                columns: new[] { "CompanyId", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_budgets_CompanyId",
                table: "acc_budgets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_bills_CompanyId_BillNumber",
                table: "acc_bills",
                columns: new[] { "CompanyId", "BillNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_bank_accounts_CompanyId_AccountNumber",
                table: "acc_bank_accounts",
                columns: new[] { "CompanyId", "AccountNumber" },
                unique: true);

            // ── 6. FKs, now that every row has a valid CompanyId. ──────────────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_acc_bank_accounts_companies_CompanyId",
                table: "acc_bank_accounts", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_bills_companies_CompanyId",
                table: "acc_bills", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_budgets_companies_CompanyId",
                table: "acc_budgets", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_invoices_companies_CompanyId",
                table: "acc_invoices", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_journalentries_companies_CompanyId",
                table: "acc_journalentries", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_numbering_sequences_companies_CompanyId",
                table: "acc_numbering_sequences", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_payments_companies_CompanyId",
                table: "acc_payments", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_vouchers_companies_CompanyId",
                table: "acc_vouchers", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Structural rollback only — does not undo the auth_user_companies backfill from
            // step 3 above (those rows are harmless to leave in place: a user simply belonging to
            // a company is not itself a problem once CompanyId columns are gone).
            migrationBuilder.DropForeignKey(name: "FK_acc_bank_accounts_companies_CompanyId", table: "acc_bank_accounts");
            migrationBuilder.DropForeignKey(name: "FK_acc_bills_companies_CompanyId", table: "acc_bills");
            migrationBuilder.DropForeignKey(name: "FK_acc_budgets_companies_CompanyId", table: "acc_budgets");
            migrationBuilder.DropForeignKey(name: "FK_acc_invoices_companies_CompanyId", table: "acc_invoices");
            migrationBuilder.DropForeignKey(name: "FK_acc_journalentries_companies_CompanyId", table: "acc_journalentries");
            migrationBuilder.DropForeignKey(name: "FK_acc_numbering_sequences_companies_CompanyId", table: "acc_numbering_sequences");
            migrationBuilder.DropForeignKey(name: "FK_acc_payments_companies_CompanyId", table: "acc_payments");
            migrationBuilder.DropForeignKey(name: "FK_acc_vouchers_companies_CompanyId", table: "acc_vouchers");

            migrationBuilder.DropIndex(name: "IX_acc_vouchers_CompanyId_VoucherNumber", table: "acc_vouchers");
            migrationBuilder.DropIndex(name: "IX_acc_payments_CompanyId_PaymentNumber", table: "acc_payments");
            migrationBuilder.DropIndex(name: "IX_acc_numbering_sequences_CompanyId_DocumentType_Prefix_Year_~", table: "acc_numbering_sequences");
            migrationBuilder.DropIndex(name: "IX_acc_journalentries_CompanyId_EntryNumber", table: "acc_journalentries");
            migrationBuilder.DropIndex(name: "IX_acc_invoices_CompanyId_InvoiceNumber", table: "acc_invoices");
            migrationBuilder.DropIndex(name: "IX_acc_budgets_CompanyId", table: "acc_budgets");
            migrationBuilder.DropIndex(name: "IX_acc_bills_CompanyId_BillNumber", table: "acc_bills");
            migrationBuilder.DropIndex(name: "IX_acc_bank_accounts_CompanyId_AccountNumber", table: "acc_bank_accounts");

            migrationBuilder.DropColumn(name: "CompanyId", table: "auth_refresh_tokens");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_vouchers");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_payments");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_numbering_sequences");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_journalentries");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_invoices");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_budgets");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_bills");
            migrationBuilder.DropColumn(name: "CompanyId", table: "acc_bank_accounts");

            migrationBuilder.CreateIndex(name: "IX_acc_vouchers_VoucherNumber", table: "acc_vouchers", column: "VoucherNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_acc_payments_PaymentNumber", table: "acc_payments", column: "PaymentNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_acc_numbering_sequences_DocumentType_Prefix_Year_Month", table: "acc_numbering_sequences", columns: new[] { "DocumentType", "Prefix", "Year", "Month" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_acc_journalentries_EntryNumber", table: "acc_journalentries", column: "EntryNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_acc_invoices_InvoiceNumber", table: "acc_invoices", column: "InvoiceNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_acc_bills_BillNumber", table: "acc_bills", column: "BillNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_acc_bank_accounts_AccountNumber", table: "acc_bank_accounts", column: "AccountNumber", unique: true);
        }
    }
}
