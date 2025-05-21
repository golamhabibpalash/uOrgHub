using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrgHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IdentityTablesModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "AspNetUserTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetUserTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedBy",
                table: "AspNetUserTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetUserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUserRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "AspNetUserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedBy",
                table: "AspNetUserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "AspNetUserLogins",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetUserLogins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUserLogins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "AspNetUserLogins",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedBy",
                table: "AspNetUserLogins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetUserClaims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedBy",
                table: "AspNetUserClaims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationRoleId",
                table: "AspNetRoleClaims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetRoleClaims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetRoleClaims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "AspNetRoleClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedBy",
                table: "AspNetRoleClaims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserTokens_ApplicationUserId",
                table: "AspNetUserTokens",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_ApplicationUserId",
                table: "AspNetUserLogins",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_ApplicationRoleId",
                table: "AspNetRoleClaims",
                column: "ApplicationRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_ApplicationRoleId",
                table: "AspNetRoleClaims",
                column: "ApplicationRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_ApplicationUserId",
                table: "AspNetUserLogins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_ApplicationUserId",
                table: "AspNetUserTokens",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_ApplicationRoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_ApplicationUserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_ApplicationUserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserTokens_ApplicationUserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserLogins_ApplicationUserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoleClaims_ApplicationRoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "ApplicationRoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "AspNetRoleClaims");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RoleId, x.UserId });
                });
        }
    }
}
