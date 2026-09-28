using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Sister-concern isolation, phase 2 (SISTER_CONCERN_PLAN.md §6) — scopes Procurement's
    /// PR → RFQ → VendorQuotation → PO → GRN chain to a CompanyId. Hand-edited after `dotnet ef
    /// migrations add`, same nullable → backfill → NOT NULL pattern as phase 1
    /// (20260921101647_UnifySisterConcernAccountsPhase1) — see that migration's remarks for why
    /// the naive EF-generated NOT NULL/Guid.Empty-default version would break the
    /// FK-to-companies step on any table with existing rows.
    /// </remarks>
    public partial class UnifySisterConcernProcurementPhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Add every CompanyId column nullable first. ──────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "proc_vendor_quotations", type: "uuid", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "proc_request_for_quotations", type: "uuid", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "proc_purchase_requisitions", type: "uuid", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "proc_purchase_orders", type: "uuid", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId", table: "proc_goods_received_notes", type: "uuid", nullable: true);

            // ── 2. Backfill every existing row to the earliest-created company (see phase 1's
            // remarks for why this is a safe no-op on a database with no companies yet). ────────
            migrationBuilder.Sql(@"
                UPDATE proc_vendor_quotations SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE proc_request_for_quotations SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE proc_purchase_requisitions SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE proc_purchase_orders SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE proc_goods_received_notes SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
            ");

            // ── 3. Now safe to tighten every backfilled column to NOT NULL. ────────────────────
            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "proc_vendor_quotations", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "proc_request_for_quotations", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "proc_purchase_requisitions", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "proc_purchase_orders", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "proc_goods_received_notes", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            // ── 4. Drop the old globally-unique number indexes, replace with (CompanyId, Number)
            // composites — two sister concerns number documents independently. ─────────────────
            migrationBuilder.DropIndex(name: "IX_proc_vendor_quotations_QuotationNumber", table: "proc_vendor_quotations");
            migrationBuilder.DropIndex(name: "IX_proc_request_for_quotations_RFQNumber", table: "proc_request_for_quotations");
            migrationBuilder.DropIndex(name: "IX_proc_purchase_requisitions_PRNumber", table: "proc_purchase_requisitions");
            migrationBuilder.DropIndex(name: "IX_proc_purchase_orders_PONumber", table: "proc_purchase_orders");
            migrationBuilder.DropIndex(name: "IX_proc_goods_received_notes_GRNNumber", table: "proc_goods_received_notes");

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendor_quotations_CompanyId_QuotationNumber",
                table: "proc_vendor_quotations",
                columns: new[] { "CompanyId", "QuotationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_request_for_quotations_CompanyId_RFQNumber",
                table: "proc_request_for_quotations",
                columns: new[] { "CompanyId", "RFQNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_requisitions_CompanyId_PRNumber",
                table: "proc_purchase_requisitions",
                columns: new[] { "CompanyId", "PRNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_orders_CompanyId_PONumber",
                table: "proc_purchase_orders",
                columns: new[] { "CompanyId", "PONumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_goods_received_notes_CompanyId_GRNNumber",
                table: "proc_goods_received_notes",
                columns: new[] { "CompanyId", "GRNNumber" },
                unique: true);

            // ── 5. FKs, now that every row has a valid CompanyId. ──────────────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_proc_goods_received_notes_companies_CompanyId",
                table: "proc_goods_received_notes", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_purchase_orders_companies_CompanyId",
                table: "proc_purchase_orders", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_purchase_requisitions_companies_CompanyId",
                table: "proc_purchase_requisitions", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_request_for_quotations_companies_CompanyId",
                table: "proc_request_for_quotations", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_vendor_quotations_companies_CompanyId",
                table: "proc_vendor_quotations", column: "CompanyId",
                principalTable: "companies", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_proc_goods_received_notes_companies_CompanyId", table: "proc_goods_received_notes");
            migrationBuilder.DropForeignKey(name: "FK_proc_purchase_orders_companies_CompanyId", table: "proc_purchase_orders");
            migrationBuilder.DropForeignKey(name: "FK_proc_purchase_requisitions_companies_CompanyId", table: "proc_purchase_requisitions");
            migrationBuilder.DropForeignKey(name: "FK_proc_request_for_quotations_companies_CompanyId", table: "proc_request_for_quotations");
            migrationBuilder.DropForeignKey(name: "FK_proc_vendor_quotations_companies_CompanyId", table: "proc_vendor_quotations");

            migrationBuilder.DropIndex(name: "IX_proc_vendor_quotations_CompanyId_QuotationNumber", table: "proc_vendor_quotations");
            migrationBuilder.DropIndex(name: "IX_proc_request_for_quotations_CompanyId_RFQNumber", table: "proc_request_for_quotations");
            migrationBuilder.DropIndex(name: "IX_proc_purchase_requisitions_CompanyId_PRNumber", table: "proc_purchase_requisitions");
            migrationBuilder.DropIndex(name: "IX_proc_purchase_orders_CompanyId_PONumber", table: "proc_purchase_orders");
            migrationBuilder.DropIndex(name: "IX_proc_goods_received_notes_CompanyId_GRNNumber", table: "proc_goods_received_notes");

            migrationBuilder.DropColumn(name: "CompanyId", table: "proc_vendor_quotations");
            migrationBuilder.DropColumn(name: "CompanyId", table: "proc_request_for_quotations");
            migrationBuilder.DropColumn(name: "CompanyId", table: "proc_purchase_requisitions");
            migrationBuilder.DropColumn(name: "CompanyId", table: "proc_purchase_orders");
            migrationBuilder.DropColumn(name: "CompanyId", table: "proc_goods_received_notes");

            migrationBuilder.CreateIndex(name: "IX_proc_vendor_quotations_QuotationNumber", table: "proc_vendor_quotations", column: "QuotationNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_proc_request_for_quotations_RFQNumber", table: "proc_request_for_quotations", column: "RFQNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_proc_purchase_requisitions_PRNumber", table: "proc_purchase_requisitions", column: "PRNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_proc_purchase_orders_PONumber", table: "proc_purchase_orders", column: "PONumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_proc_goods_received_notes_GRNNumber", table: "proc_goods_received_notes", column: "GRNNumber", unique: true);
        }
    }
}
