using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountGroupHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentAccountGroupId",
                table: "acc_accountgroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_accountgroups_ParentAccountGroupId",
                table: "acc_accountgroups",
                column: "ParentAccountGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_acc_accountgroups_acc_accountgroups_ParentAccountGroupId",
                table: "acc_accountgroups",
                column: "ParentAccountGroupId",
                principalTable: "acc_accountgroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_accountgroups_acc_accountgroups_ParentAccountGroupId",
                table: "acc_accountgroups");

            migrationBuilder.DropIndex(
                name: "IX_acc_accountgroups_ParentAccountGroupId",
                table: "acc_accountgroups");

            migrationBuilder.DropColumn(
                name: "ParentAccountGroupId",
                table: "acc_accountgroups");
        }
    }
}
