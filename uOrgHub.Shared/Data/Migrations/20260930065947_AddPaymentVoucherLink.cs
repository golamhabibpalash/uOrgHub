using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentVoucherLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VoucherId",
                table: "acc_payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_payments_VoucherId",
                table: "acc_payments",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_acc_payments_acc_vouchers_VoucherId",
                table: "acc_payments",
                column: "VoucherId",
                principalTable: "acc_vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_payments_acc_vouchers_VoucherId",
                table: "acc_payments");

            migrationBuilder.DropIndex(
                name: "IX_acc_payments_VoucherId",
                table: "acc_payments");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "acc_payments");
        }
    }
}
