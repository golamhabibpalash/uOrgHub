using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acc_vouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VoucherType = table.Column<int>(type: "integer", nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Section = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DebitAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreparedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReceivedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SubmittedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PostedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_vouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_vouchers_acc_chartofaccounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_vouchers_acc_chartofaccounts_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_vouchers_acc_fiscalyears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "acc_fiscalyears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_vouchers_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_CreditAccountId",
                table: "acc_vouchers",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_DebitAccountId",
                table: "acc_vouchers",
                column: "DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_FiscalYearId",
                table: "acc_vouchers",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_JournalEntryId",
                table: "acc_vouchers",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vouchers_VoucherNumber",
                table: "acc_vouchers",
                column: "VoucherNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acc_vouchers");
        }
    }
}
