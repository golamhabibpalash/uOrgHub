using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkProjectsToAccountsViaCostCenters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "proj_project_expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "acc_journalentrylines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "acc_cost_centers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_journalentrylines_CostCenterId",
                table: "acc_journalentrylines",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_cost_centers_ProjectId",
                table: "acc_cost_centers",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_acc_journalentrylines_acc_cost_centers_CostCenterId",
                table: "acc_journalentrylines",
                column: "CostCenterId",
                principalTable: "acc_cost_centers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_journalentrylines_acc_cost_centers_CostCenterId",
                table: "acc_journalentrylines");

            migrationBuilder.DropIndex(
                name: "IX_acc_journalentrylines_CostCenterId",
                table: "acc_journalentrylines");

            migrationBuilder.DropIndex(
                name: "IX_acc_cost_centers_ProjectId",
                table: "acc_cost_centers");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "proj_project_expenses");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "acc_journalentrylines");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "acc_cost_centers");
        }
    }
}
