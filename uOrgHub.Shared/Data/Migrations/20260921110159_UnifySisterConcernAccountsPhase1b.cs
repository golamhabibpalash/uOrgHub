using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Sister-concern isolation, phase 1b (SISTER_CONCERN_PLAN.md) — closes the gap left by
    /// phase 1 (20260921101647_UnifySisterConcernAccountsPhase1): scopes FiscalYear and
    /// CostCenter to a company too, so "the current fiscal year" and cost-center codes are no
    /// longer shared across sister concerns. Same nullable → backfill → NOT NULL pattern as
    /// phase 1 — see that migration's remarks for why the naive EF-generated
    /// NOT NULL/Guid.Empty-default version would break the FK-to-companies step.
    /// </remarks>
    public partial class UnifySisterConcernAccountsPhase1b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_acc_cost_centers_Code",
                table: "acc_cost_centers");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_fiscalyears",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "acc_cost_centers",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE acc_fiscalyears SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
                UPDATE acc_cost_centers SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_fiscalyears", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "acc_cost_centers", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_fiscalyears_CompanyId",
                table: "acc_fiscalyears",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_cost_centers_CompanyId_Code",
                table: "acc_cost_centers",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_cost_centers_companies_CompanyId",
                table: "acc_cost_centers",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_fiscalyears_companies_CompanyId",
                table: "acc_fiscalyears",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_cost_centers_companies_CompanyId",
                table: "acc_cost_centers");

            migrationBuilder.DropForeignKey(
                name: "FK_acc_fiscalyears_companies_CompanyId",
                table: "acc_fiscalyears");

            migrationBuilder.DropIndex(
                name: "IX_acc_fiscalyears_CompanyId",
                table: "acc_fiscalyears");

            migrationBuilder.DropIndex(
                name: "IX_acc_cost_centers_CompanyId_Code",
                table: "acc_cost_centers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "acc_fiscalyears");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "acc_cost_centers");

            migrationBuilder.CreateIndex(
                name: "IX_acc_cost_centers_Code",
                table: "acc_cost_centers",
                column: "Code",
                unique: true);
        }
    }
}
