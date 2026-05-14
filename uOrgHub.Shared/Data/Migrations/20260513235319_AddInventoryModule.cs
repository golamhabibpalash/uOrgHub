using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inv_inventory_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_inv_inventory_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inv_units_of_measure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_inv_units_of_measure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inv_warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_inv_warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inv_inventory_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_inv_inventory_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_inventory_categories_inv_inventory_categories_ParentCat~",
                        column: x => x.ParentCategoryId,
                        principalTable: "inv_inventory_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_inventory_categories_inv_inventory_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "inv_inventory_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inv_attribute_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    PredefinedValues = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_inv_attribute_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_attribute_definitions_inv_inventory_categories_Category~",
                        column: x => x.CategoryId,
                        principalTable: "inv_inventory_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "inv_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReorderLevel = table.Column<decimal>(type: "numeric", nullable: false),
                    StandardCost = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_inv_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_items_inv_inventory_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "inv_inventory_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_items_inv_inventory_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "inv_inventory_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_items_inv_units_of_measure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "inv_units_of_measure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inv_item_variants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SKU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VariantName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AttributeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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
                    table.PrimaryKey("PK_inv_item_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_item_variants_inv_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "inv_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inv_stock_balances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_inv_stock_balances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_stock_balances_inv_item_variants_ItemVariantId",
                        column: x => x.ItemVariantId,
                        principalTable: "inv_item_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_stock_balances_inv_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "inv_warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inv_stock_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromWarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_inv_stock_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_stock_transactions_inv_item_variants_ItemVariantId",
                        column: x => x.ItemVariantId,
                        principalTable: "inv_item_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_stock_transactions_inv_warehouses_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "inv_warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_stock_transactions_inv_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "inv_warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inv_variant_attributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttributeDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_inv_variant_attributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inv_variant_attributes_inv_attribute_definitions_AttributeD~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "inv_attribute_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inv_variant_attributes_inv_item_variants_ItemVariantId",
                        column: x => x.ItemVariantId,
                        principalTable: "inv_item_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inv_attribute_definitions_CategoryId",
                table: "inv_attribute_definitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_inventory_categories_Code",
                table: "inv_inventory_categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_inventory_categories_ParentCategoryId",
                table: "inv_inventory_categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_inventory_categories_TypeId",
                table: "inv_inventory_categories",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_inventory_types_Code",
                table: "inv_inventory_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_item_variants_ItemId",
                table: "inv_item_variants",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_item_variants_SKU",
                table: "inv_item_variants",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_items_CategoryId",
                table: "inv_items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_items_ItemCode",
                table: "inv_items",
                column: "ItemCode",
                unique: true,
                filter: "\"ItemCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inv_items_TypeId",
                table: "inv_items",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_items_UnitOfMeasureId",
                table: "inv_items",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_balances_ItemVariantId_WarehouseId",
                table: "inv_stock_balances",
                columns: new[] { "ItemVariantId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_balances_WarehouseId",
                table: "inv_stock_balances",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_transactions_FromWarehouseId",
                table: "inv_stock_transactions",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_transactions_ItemVariantId",
                table: "inv_stock_transactions",
                column: "ItemVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_transactions_TransactionNumber",
                table: "inv_stock_transactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_stock_transactions_WarehouseId",
                table: "inv_stock_transactions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_units_of_measure_Abbreviation",
                table: "inv_units_of_measure",
                column: "Abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inv_variant_attributes_AttributeDefinitionId",
                table: "inv_variant_attributes",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_variant_attributes_ItemVariantId",
                table: "inv_variant_attributes",
                column: "ItemVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_inv_warehouses_Code",
                table: "inv_warehouses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inv_stock_balances");

            migrationBuilder.DropTable(
                name: "inv_stock_transactions");

            migrationBuilder.DropTable(
                name: "inv_variant_attributes");

            migrationBuilder.DropTable(
                name: "inv_warehouses");

            migrationBuilder.DropTable(
                name: "inv_attribute_definitions");

            migrationBuilder.DropTable(
                name: "inv_item_variants");

            migrationBuilder.DropTable(
                name: "inv_items");

            migrationBuilder.DropTable(
                name: "inv_inventory_categories");

            migrationBuilder.DropTable(
                name: "inv_units_of_measure");

            migrationBuilder.DropTable(
                name: "inv_inventory_types");
        }
    }
}
