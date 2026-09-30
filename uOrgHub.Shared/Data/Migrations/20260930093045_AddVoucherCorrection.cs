using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CorrectsVoucherId",
                table: "acc_vouchers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversalJournalEntryId",
                table: "acc_vouchers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                table: "acc_vouchers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedAt",
                table: "acc_vouchers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversedBy",
                table: "acc_vouchers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_CorrectsVoucherId",
                table: "acc_vouchers",
                column: "CorrectsVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_ReversalJournalEntryId",
                table: "acc_vouchers",
                column: "ReversalJournalEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_acc_vouchers_acc_journalentries_ReversalJournalEntryId",
                table: "acc_vouchers",
                column: "ReversalJournalEntryId",
                principalTable: "acc_journalentries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_vouchers_acc_vouchers_CorrectsVoucherId",
                table: "acc_vouchers",
                column: "CorrectsVoucherId",
                principalTable: "acc_vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_vouchers_acc_journalentries_ReversalJournalEntryId",
                table: "acc_vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_acc_vouchers_acc_vouchers_CorrectsVoucherId",
                table: "acc_vouchers");

            migrationBuilder.DropIndex(
                name: "IX_acc_vouchers_CorrectsVoucherId",
                table: "acc_vouchers");

            migrationBuilder.DropIndex(
                name: "IX_acc_vouchers_ReversalJournalEntryId",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "CorrectsVoucherId",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "ReversalJournalEntryId",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "ReversedAt",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "ReversedBy",
                table: "acc_vouchers");
        }
    }
}
