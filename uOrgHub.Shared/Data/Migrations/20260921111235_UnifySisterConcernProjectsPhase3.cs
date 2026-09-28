using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Sister-concern isolation, phase 3 (SISTER_CONCERN_PLAN.md §6) — scopes `proj_projects` to
    /// a CompanyId. `Project` is the module's single anchor; every other Projects entity reaches
    /// it via ProjectId and inherits scoping through it, so this is the only table phase 3
    /// touches. Same nullable → backfill → NOT NULL pattern as phases 1/2 — see
    /// 20260921101647_UnifySisterConcernAccountsPhase1's remarks for why the naive EF-generated
    /// NOT NULL/Guid.Empty-default version would break the FK-to-companies step on any table
    /// with existing rows.
    /// </remarks>
    public partial class UnifySisterConcernProjectsPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "proj_projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE proj_projects SET ""CompanyId"" = (SELECT ""Id"" FROM companies ORDER BY ""CreatedAt"" LIMIT 1) WHERE ""CompanyId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId", table: "proj_projects", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_proj_projects_ProjectCode",
                table: "proj_projects");

            migrationBuilder.CreateIndex(
                name: "IX_proj_projects_CompanyId_ProjectCode",
                table: "proj_projects",
                columns: new[] { "CompanyId", "ProjectCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_proj_projects_companies_CompanyId",
                table: "proj_projects",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_proj_projects_companies_CompanyId",
                table: "proj_projects");

            migrationBuilder.DropIndex(
                name: "IX_proj_projects_CompanyId_ProjectCode",
                table: "proj_projects");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "proj_projects");

            migrationBuilder.CreateIndex(
                name: "IX_proj_projects_ProjectCode",
                table: "proj_projects",
                column: "ProjectCode",
                unique: true);
        }
    }
}
