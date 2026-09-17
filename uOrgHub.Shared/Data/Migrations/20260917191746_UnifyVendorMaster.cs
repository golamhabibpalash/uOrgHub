using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifyVendorMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_bills_acc_vendors_VendorId",
                table: "acc_bills");

            migrationBuilder.DropForeignKey(
                name: "FK_acc_payments_acc_vendors_VendorId",
                table: "acc_payments");

            migrationBuilder.DropForeignKey(
                name: "FK_proc_purchase_orders_proc_vendors_VendorId",
                table: "proc_purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_proc_vendor_quotations_proc_vendors_VendorId",
                table: "proc_vendor_quotations");

            migrationBuilder.DropTable(
                name: "acc_vendors");

            migrationBuilder.DropTable(
                name: "proc_vendors");

            migrationBuilder.CreateTable(
                name: "vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TIN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BIN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TradeLicense = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VendorType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PayableAccountId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vendors_acc_chartofaccounts_PayableAccountId",
                        column: x => x.PayableAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vendors_PayableAccountId",
                table: "vendors",
                column: "PayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_vendors_VendorCode",
                table: "vendors",
                column: "VendorCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_bills_vendors_VendorId",
                table: "acc_bills",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_payments_vendors_VendorId",
                table: "acc_payments",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_purchase_orders_vendors_VendorId",
                table: "proc_purchase_orders",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_vendor_quotations_vendors_VendorId",
                table: "proc_vendor_quotations",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_bills_vendors_VendorId",
                table: "acc_bills");

            migrationBuilder.DropForeignKey(
                name: "FK_acc_payments_vendors_VendorId",
                table: "acc_payments");

            migrationBuilder.DropForeignKey(
                name: "FK_proc_purchase_orders_vendors_VendorId",
                table: "proc_purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_proc_vendor_quotations_vendors_VendorId",
                table: "proc_vendor_quotations");

            migrationBuilder.DropTable(
                name: "vendors");

            migrationBuilder.CreateTable(
                name: "acc_vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayableAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PaymentTermsDays = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    VendorCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_vendors_acc_chartofaccounts_PayableAccountId",
                        column: x => x.PayableAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proc_vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BIN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TIN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TradeLicense = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    VendorCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VendorType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_vendors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acc_vendors_PayableAccountId",
                table: "acc_vendors",
                column: "PayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_vendors_VendorCode",
                table: "acc_vendors",
                column: "VendorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendors_VendorCode",
                table: "proc_vendors",
                column: "VendorCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_bills_acc_vendors_VendorId",
                table: "acc_bills",
                column: "VendorId",
                principalTable: "acc_vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_payments_acc_vendors_VendorId",
                table: "acc_payments",
                column: "VendorId",
                principalTable: "acc_vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_purchase_orders_proc_vendors_VendorId",
                table: "proc_purchase_orders",
                column: "VendorId",
                principalTable: "proc_vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proc_vendor_quotations_proc_vendors_VendorId",
                table: "proc_vendor_quotations",
                column: "VendorId",
                principalTable: "proc_vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
