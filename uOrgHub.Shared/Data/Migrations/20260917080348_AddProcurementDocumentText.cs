using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementDocumentText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentEditedAt",
                table: "proc_request_for_quotations",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentText",
                table: "proc_request_for_quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentEditedAt",
                table: "proc_purchase_requisitions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentText",
                table: "proc_purchase_requisitions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentEditedAt",
                table: "proc_request_for_quotations");

            migrationBuilder.DropColumn(
                name: "DocumentText",
                table: "proc_request_for_quotations");

            migrationBuilder.DropColumn(
                name: "DocumentEditedAt",
                table: "proc_purchase_requisitions");

            migrationBuilder.DropColumn(
                name: "DocumentText",
                table: "proc_purchase_requisitions");
        }
    }
}
