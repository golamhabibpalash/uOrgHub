using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCreatedBy1Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy1",
                table: "acc_journalentries");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "acc_journalentries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "acc_journalentries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy1",
                table: "acc_journalentries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
