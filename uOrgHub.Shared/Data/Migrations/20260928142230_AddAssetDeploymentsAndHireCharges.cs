using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetDeploymentsAndHireCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HireExpenseAccountId",
                table: "acc_asset_categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HireRecoveryAccountId",
                table: "acc_asset_categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "acc_asset_deployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixedAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChargeMode = table.Column<int>(type: "integer", nullable: false),
                    RateUnit = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PreviousLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReturnNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_acc_asset_deployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_asset_deployments_acc_cost_centers_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "acc_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_asset_deployments_acc_fixed_assets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "acc_fixed_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_asset_deployments_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_hire_charge_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_acc_hire_charge_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_hire_charge_runs_acc_journalentries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "acc_journalentries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_acc_hire_charge_runs_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acc_hire_charge_run_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HireChargeRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetDeploymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    RateUnit = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_acc_hire_charge_run_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acc_hire_charge_run_lines_acc_asset_deployments_AssetDeploy~",
                        column: x => x.AssetDeploymentId,
                        principalTable: "acc_asset_deployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acc_hire_charge_run_lines_acc_hire_charge_runs_HireChargeRu~",
                        column: x => x.HireChargeRunId,
                        principalTable: "acc_hire_charge_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_categories_HireExpenseAccountId",
                table: "acc_asset_categories",
                column: "HireExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_categories_HireRecoveryAccountId",
                table: "acc_asset_categories",
                column: "HireRecoveryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_deployments_CompanyId",
                table: "acc_asset_deployments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_deployments_CostCenterId",
                table: "acc_asset_deployments",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_deployments_FixedAssetId_StartDate",
                table: "acc_asset_deployments",
                columns: new[] { "FixedAssetId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_acc_asset_deployments_ProjectId",
                table: "acc_asset_deployments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_hire_charge_run_lines_AssetDeploymentId",
                table: "acc_hire_charge_run_lines",
                column: "AssetDeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_hire_charge_run_lines_HireChargeRunId",
                table: "acc_hire_charge_run_lines",
                column: "HireChargeRunId");

            migrationBuilder.CreateIndex(
                name: "IX_acc_hire_charge_runs_CompanyId_FromDate_ToDate",
                table: "acc_hire_charge_runs",
                columns: new[] { "CompanyId", "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_acc_hire_charge_runs_CompanyId_RunNumber",
                table: "acc_hire_charge_runs",
                columns: new[] { "CompanyId", "RunNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_acc_hire_charge_runs_JournalEntryId",
                table: "acc_hire_charge_runs",
                column: "JournalEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_acc_asset_categories_acc_chartofaccounts_HireExpenseAccount~",
                table: "acc_asset_categories",
                column: "HireExpenseAccountId",
                principalTable: "acc_chartofaccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_acc_asset_categories_acc_chartofaccounts_HireRecoveryAccoun~",
                table: "acc_asset_categories",
                column: "HireRecoveryAccountId",
                principalTable: "acc_chartofaccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_acc_asset_categories_acc_chartofaccounts_HireExpenseAccount~",
                table: "acc_asset_categories");

            migrationBuilder.DropForeignKey(
                name: "FK_acc_asset_categories_acc_chartofaccounts_HireRecoveryAccoun~",
                table: "acc_asset_categories");

            migrationBuilder.DropTable(
                name: "acc_hire_charge_run_lines");

            migrationBuilder.DropTable(
                name: "acc_asset_deployments");

            migrationBuilder.DropTable(
                name: "acc_hire_charge_runs");

            migrationBuilder.DropIndex(
                name: "IX_acc_asset_categories_HireExpenseAccountId",
                table: "acc_asset_categories");

            migrationBuilder.DropIndex(
                name: "IX_acc_asset_categories_HireRecoveryAccountId",
                table: "acc_asset_categories");

            migrationBuilder.DropColumn(
                name: "HireExpenseAccountId",
                table: "acc_asset_categories");

            migrationBuilder.DropColumn(
                name: "HireRecoveryAccountId",
                table: "acc_asset_categories");
        }
    }
}
