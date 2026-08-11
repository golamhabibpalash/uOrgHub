using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherProjectAndCostCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "acc_vouchers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "acc_vouchers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_CostCenterId",
                table: "acc_vouchers",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_ProjectId",
                table: "acc_vouchers",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_acc_vouchers_acc_cost_centers_CostCenterId",
                table: "acc_vouchers",
                column: "CostCenterId",
                principalTable: "acc_cost_centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_vouchers_acc_cost_centers_CostCenterId",
                table: "acc_vouchers");

            migrationBuilder.DropIndex(
                name: "IX_acc_vouchers_CostCenterId",
                table: "acc_vouchers");

            migrationBuilder.DropIndex(
                name: "IX_acc_vouchers_ProjectId",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "acc_vouchers");
        }
    }
}
