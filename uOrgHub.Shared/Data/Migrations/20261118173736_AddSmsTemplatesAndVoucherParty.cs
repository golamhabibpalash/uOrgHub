using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsTemplatesAndVoucherParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceReference",
                table: "sms_send_requests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartyId",
                table: "acc_vouchers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartyType",
                table: "acc_vouchers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendSmsOnPost",
                table: "acc_vouchers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "sms_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_sms_templates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sms_templates_CompanyId_EventKey",
                table: "sms_templates",
                columns: new[] { "CompanyId", "EventKey" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_templates");

            migrationBuilder.DropColumn(
                name: "SourceReference",
                table: "sms_send_requests");

            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "PartyType",
                table: "acc_vouchers");

            migrationBuilder.DropColumn(
                name: "SendSmsOnPost",
                table: "acc_vouchers");
        }
    }
}
