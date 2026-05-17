using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyTagLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagLine",
                table: "companies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagLine",
                table: "companies");
        }
    }
}
