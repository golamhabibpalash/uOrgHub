using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_acc_journalentries_EntryDate",
                table: "acc_journalentries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_acc_journalentries_Status",
                table: "acc_journalentries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_acc_journalentries_EntryDate",
                table: "acc_journalentries");

            migrationBuilder.DropIndex(
                name: "IX_acc_journalentries_Status",
                table: "acc_journalentries");
        }
    }
}
