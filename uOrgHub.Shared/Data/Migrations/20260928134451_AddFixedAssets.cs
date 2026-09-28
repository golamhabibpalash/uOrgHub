using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acc_asset_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DepreciationMethod = table.Column<int>(type: "integer", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    SalvageValuePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    AssetAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccumulatedDepreciationAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepreciationExpenseAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_acc_asset_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_asset_categories_acc_chartofaccounts_AccumulatedDepreci~",
                        column: x => x.AccumulatedDepreciationAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_asset_categories_acc_chartofaccounts_AssetAccountId",
                        column: x => x.AssetAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_asset_categories_acc_chartofaccounts_DepreciationExpens~",
                        column: x => x.DepreciationExpenseAccountId,
                        principalTable: "acc_chartofaccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_asset_categories_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_depreciation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_acc_depreciation_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_depreciation_runs_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_depreciation_runs_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_fixed_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChassisNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EngineNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DepreciationStartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PurchaseCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalvageValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    DepreciationMethod = table.Column<int>(type: "integer", nullable: false),
                    OpeningAccumulatedDepreciation = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AccumulatedDepreciation = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LastDepreciationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_acc_fixed_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_fixed_assets_acc_asset_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "acc_asset_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_fixed_assets_acc_bills_BillId",
                        column: x => x.BillId,
                        principalTable: "acc_bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_fixed_assets_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_fixed_assets_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_fixed_assets_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "acc_depreciation_run_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepreciationRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixedAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Months = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AccumulatedBefore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AccumulatedAfter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PreviousLastDepreciationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_acc_depreciation_run_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_depreciation_run_lines_acc_depreciation_runs_Depreciati~",
                        column: x => x.DepreciationRunId,
                        principalTable: "acc_depreciation_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_acc_depreciation_run_lines_acc_fixed_assets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "acc_fixed_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_categories_AccumulatedDepreciationAccountId",
                table: "acc_asset_categories",
                column: "AccumulatedDepreciationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_categories_AssetAccountId",
                table: "acc_asset_categories",
                column: "AssetAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_categories_CompanyId_Code",
                table: "acc_asset_categories",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_categories_DepreciationExpenseAccountId",
                table: "acc_asset_categories",
                column: "DepreciationExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_depreciation_run_lines_DepreciationRunId",
                table: "acc_depreciation_run_lines",
                column: "DepreciationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_depreciation_run_lines_FixedAssetId",
                table: "acc_depreciation_run_lines",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_depreciation_runs_CompanyId_PeriodYear_PeriodMonth",
                table: "acc_depreciation_runs",
                columns: new[] { "CompanyId", "PeriodYear", "PeriodMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_acc_depreciation_runs_CompanyId_RunNumber",
                table: "acc_depreciation_runs",
                columns: new[] { "CompanyId", "RunNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_depreciation_runs_JournalEntryId",
                table: "acc_depreciation_runs",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_fixed_assets_BillId",
                table: "acc_fixed_assets",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_fixed_assets_CategoryId",
                table: "acc_fixed_assets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_fixed_assets_CompanyId_AssetCode",
                table: "acc_fixed_assets",
                columns: new[] { "CompanyId", "AssetCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_fixed_assets_CostCenterId",
                table: "acc_fixed_assets",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_fixed_assets_VendorId",
                table: "acc_fixed_assets",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acc_depreciation_run_lines");

            migrationBuilder.DropTable(
                name: "acc_depreciation_runs");

            migrationBuilder.DropTable(
                name: "acc_fixed_assets");

            migrationBuilder.DropTable(
                name: "acc_asset_categories");
        }
    }
}
