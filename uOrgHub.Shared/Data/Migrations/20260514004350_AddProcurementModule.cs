using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proc_purchase_requisitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PRNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PRDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_purchase_requisitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "proc_vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TradeLicense = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TIN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BIN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VendorType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "proc_purchase_requisition_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PRId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstimatedTotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_purchase_requisition_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_purchase_requisition_items_proc_purchase_requisitions_~",
                        column: x => x.PRId,
                        principalTable: "proc_purchase_requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proc_request_for_quotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RFQNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RFQDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PRId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_request_for_quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_request_for_quotations_proc_purchase_requisitions_PRId",
                        column: x => x.PRId,
                        principalTable: "proc_purchase_requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "proc_rfq_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RFQId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_rfq_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_rfq_items_proc_request_for_quotations_RFQId",
                        column: x => x.RFQId,
                        principalTable: "proc_request_for_quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proc_vendor_quotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RFQId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    PaymentTerms = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_vendor_quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_vendor_quotations_proc_request_for_quotations_RFQId",
                        column: x => x.RFQId,
                        principalTable: "proc_request_for_quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proc_vendor_quotations_proc_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "proc_vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proc_purchase_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PONumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PODate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PRId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentTerms = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_purchase_orders_proc_purchase_requisitions_PRId",
                        column: x => x.PRId,
                        principalTable: "proc_purchase_requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_proc_purchase_orders_proc_vendor_quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "proc_vendor_quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_proc_purchase_orders_proc_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "proc_vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proc_vendor_quotation_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RFQItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_vendor_quotation_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_vendor_quotation_items_proc_rfq_items_RFQItemId",
                        column: x => x.RFQItemId,
                        principalTable: "proc_rfq_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proc_vendor_quotation_items_proc_vendor_quotations_Quotatio~",
                        column: x => x.QuotationId,
                        principalTable: "proc_vendor_quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proc_goods_received_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GRNNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    POId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_goods_received_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_goods_received_notes_proc_purchase_orders_POId",
                        column: x => x.POId,
                        principalTable: "proc_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proc_purchase_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    POId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_purchase_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_purchase_order_items_proc_purchase_orders_POId",
                        column: x => x.POId,
                        principalTable: "proc_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proc_grn_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GRNId = table.Column<Guid>(type: "uuid", nullable: false),
                    POItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    RejectedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_grn_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proc_grn_items_proc_goods_received_notes_GRNId",
                        column: x => x.GRNId,
                        principalTable: "proc_goods_received_notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_proc_grn_items_proc_purchase_order_items_POItemId",
                        column: x => x.POItemId,
                        principalTable: "proc_purchase_order_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proc_goods_received_notes_GRNNumber",
                table: "proc_goods_received_notes",
                column: "GRNNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_goods_received_notes_POId",
                table: "proc_goods_received_notes",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_grn_items_GRNId",
                table: "proc_grn_items",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_grn_items_POItemId",
                table: "proc_grn_items",
                column: "POItemId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_order_items_POId",
                table: "proc_purchase_order_items",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_orders_PONumber",
                table: "proc_purchase_orders",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_orders_PRId",
                table: "proc_purchase_orders",
                column: "PRId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_orders_QuotationId",
                table: "proc_purchase_orders",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_orders_VendorId",
                table: "proc_purchase_orders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_requisition_items_PRId",
                table: "proc_purchase_requisition_items",
                column: "PRId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_purchase_requisitions_PRNumber",
                table: "proc_purchase_requisitions",
                column: "PRNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_request_for_quotations_PRId",
                table: "proc_request_for_quotations",
                column: "PRId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_request_for_quotations_RFQNumber",
                table: "proc_request_for_quotations",
                column: "RFQNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_rfq_items_RFQId",
                table: "proc_rfq_items",
                column: "RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendor_quotation_items_QuotationId",
                table: "proc_vendor_quotation_items",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendor_quotation_items_RFQItemId",
                table: "proc_vendor_quotation_items",
                column: "RFQItemId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendor_quotations_QuotationNumber",
                table: "proc_vendor_quotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendor_quotations_RFQId_VendorId",
                table: "proc_vendor_quotations",
                columns: new[] { "RFQId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendor_quotations_VendorId",
                table: "proc_vendor_quotations",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_proc_vendors_VendorCode",
                table: "proc_vendors",
                column: "VendorCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proc_grn_items");

            migrationBuilder.DropTable(
                name: "proc_purchase_requisition_items");

            migrationBuilder.DropTable(
                name: "proc_vendor_quotation_items");

            migrationBuilder.DropTable(
                name: "proc_goods_received_notes");

            migrationBuilder.DropTable(
                name: "proc_purchase_order_items");

            migrationBuilder.DropTable(
                name: "proc_rfq_items");

            migrationBuilder.DropTable(
                name: "proc_purchase_orders");

            migrationBuilder.DropTable(
                name: "proc_vendor_quotations");

            migrationBuilder.DropTable(
                name: "proc_request_for_quotations");

            migrationBuilder.DropTable(
                name: "proc_vendors");

            migrationBuilder.DropTable(
                name: "proc_purchase_requisitions");
        }
    }
}
