using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Sister-concern isolation, phase 4a (SISTER_CONCERN_PLAN.md §6) — scopes Inventory's
    /// `Warehouse`, `StockBalance`, and `StockTransaction` to a CompanyId. Unlike the
    /// Accounts/Procurement/Projects line-item children, StockBalance/StockTransaction get their
    /// own CompanyId rather than inheriting scoping through WarehouseId alone: both have
    /// independent list/query handlers (StockBalanceQueries.cs, StockTransactionQueries.cs) that
    /// never join through Warehouse (see Warehouse.cs's comment). Same nullable → backfill →
    /// NOT NULL pattern as phases 1-3 — see 20260921101647_UnifySisterConcernAccountsPhase1's
    /// remarks for why the naive EF-generated NOT NULL/Guid.Empty-default version would break
    /// the FK-to-companies step on any table with existing rows.
    /// </remarks>
    public partial class UnifySisterConcernInventoryPhase4a : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Add every CompanyId column nullable first. ──────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "inv_warehouses", type: "uuid", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "inv_stock_transactions", type: "uuid", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "inv_stock_balances", type: "uuid", nullable: true);

            // ── 2. Backfill every existing row to the earliest-created company. ────────────────
            migrationBuilder.Sql(@"
                UPDATE inv_warehouses SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE inv_stock_transactions SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE inv_stock_balances SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
            ");

            // ── 3. Now safe to tighten every backfilled column to NOT NULL. ────────────────────
            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "inv_warehouses", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "inv_stock_transactions", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "inv_stock_balances", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            // ── 4. Drop the old globally-unique indexes, replace with (CompanyId, ...) composites
            // where a number/code is involved. ──────────────────────────────────────────────────
            migrationBuilder.DropIndex(name: "IX_inv_warehouses_Code", table: "inv_warehouses");
            migrationBuilder.DropIndex(name: "IX_inv_stock_transactions_TransactionNumber", table: "inv_stock_transactions");

            migrationBuilder.CreateIndex(
                name: "IX_inv_warehouses_CompanyId_Code",
                table: "inv_warehouses",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_transactions_CompanyId_TransactionNumber",
                table: "inv_stock_transactions",
                columns: new[] { "CompanyId", "TransactionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_balances_CompanyId",
                table: "inv_stock_balances",
                column: "CompanyId");

            // ── 5. FKs, now that every row has a valid CompanyId. ──────────────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_inv_stock_balances_companies_CompanyId",
                table: "inv_stock_balances", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_inv_stock_transactions_companies_CompanyId",
                table: "inv_stock_transactions", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_inv_warehouses_companies_CompanyId",
                table: "inv_warehouses", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_inv_stock_balances_companies_CompanyId", table: "inv_stock_balances");
            migrationBuilder.DropForeignKey(name: "FK_inv_stock_transactions_companies_CompanyId", table: "inv_stock_transactions");
            migrationBuilder.DropForeignKey(name: "FK_inv_warehouses_companies_CompanyId", table: "inv_warehouses");

            migrationBuilder.DropIndex(name: "IX_inv_warehouses_CompanyId_Code", table: "inv_warehouses");
            migrationBuilder.DropIndex(name: "IX_inv_stock_transactions_CompanyId_TransactionNumber", table: "inv_stock_transactions");
            migrationBuilder.DropIndex(name: "IX_inv_stock_balances_CompanyId", table: "inv_stock_balances");

            migrationBuilder.DropColumn(name: "CompanyId", table: "inv_warehouses");
            migrationBuilder.DropColumn(name: "CompanyId", table: "inv_stock_transactions");
            migrationBuilder.DropColumn(name: "CompanyId", table: "inv_stock_balances");

            migrationBuilder.CreateIndex(name: "IX_inv_warehouses_Code", table: "inv_warehouses", column: "Code", unique: true);
            migrationBuilder.CreateIndex(name: "IX_inv_stock_transactions_TransactionNumber", table: "inv_stock_transactions", column: "TransactionNumber", unique: true);
        }
    }
}
