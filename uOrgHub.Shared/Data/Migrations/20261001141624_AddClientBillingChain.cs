using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientBillingChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "proj_ra_bills",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultRevenueAccountId",
                table: "proj_projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "proj_clients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoneyReceiptNumber",
                table: "acc_payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "proj_retention_releases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_proj_retention_releases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_retention_releases_acc_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "acc_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proj_retention_releases_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proj_retention_releases_proj_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "proj_projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proj_ra_bills_InvoiceId",
                table: "proj_ra_bills",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_proj_projects_DefaultRevenueAccountId",
                table: "proj_projects",
                column: "DefaultRevenueAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_proj_clients_CustomerId",
                table: "proj_clients",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_proj_retention_releases_CompanyId",
                table: "proj_retention_releases",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_proj_retention_releases_InvoiceId",
                table: "proj_retention_releases",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_proj_retention_releases_ProjectId",
                table: "proj_retention_releases",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_proj_clients_acc_customers_CustomerId",
                table: "proj_clients",
                column: "CustomerId",
                principalTable: "acc_customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_proj_projects_acc_chartofaccounts_DefaultRevenueAccountId",
                table: "proj_projects",
                column: "DefaultRevenueAccountId",
                principalTable: "acc_chartofaccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_proj_ra_bills_acc_invoices_InvoiceId",
                table: "proj_ra_bills",
                column: "InvoiceId",
                principalTable: "acc_invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_proj_clients_acc_customers_CustomerId",
                table: "proj_clients");

            migrationBuilder.DropForeignKey(
                name: "FK_proj_projects_acc_chartofaccounts_DefaultRevenueAccountId",
                table: "proj_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_proj_ra_bills_acc_invoices_InvoiceId",
                table: "proj_ra_bills");

            migrationBuilder.DropTable(
                name: "proj_retention_releases");

            migrationBuilder.DropIndex(
                name: "IX_proj_ra_bills_InvoiceId",
                table: "proj_ra_bills");

            migrationBuilder.DropIndex(
                name: "IX_proj_projects_DefaultRevenueAccountId",
                table: "proj_projects");

            migrationBuilder.DropIndex(
                name: "IX_proj_clients_CustomerId",
                table: "proj_clients");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "proj_ra_bills");

            migrationBuilder.DropColumn(
                name: "DefaultRevenueAccountId",
                table: "proj_projects");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "proj_clients");

            migrationBuilder.DropColumn(
                name: "MoneyReceiptNumber",
                table: "acc_payments");
        }
    }
}
