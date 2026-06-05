using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeUpazila : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Upazila",
                table: "hr_employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Upazila",
                table: "hr_employees");
        }
    }
}
