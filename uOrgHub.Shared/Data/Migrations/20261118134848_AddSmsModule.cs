using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sms_gateways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SenderId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SettingsJson = table.Column<string>(type: "text", nullable: true),
                    CredentialsCipher = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    RatePerSegment = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    MaxSendsPerSecond = table.Column<int>(type: "integer", nullable: false),
                    LastBalance = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    BalanceUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastProviderRate = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    LastBalanceSyncedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastBalanceError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastTestSucceeded = table.Column<bool>(type: "boolean", nullable: true),
                    LastTestMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_sms_gateways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sms_send_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GatewayId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageText = table.Column<string>(type: "text", nullable: false),
                    Encoding = table.Column<int>(type: "integer", nullable: false),
                    SegmentsPerRecipient = table.Column<int>(type: "integer", nullable: false),
                    RecipientCount = table.Column<int>(type: "integer", nullable: false),
                    TotalSegments = table.Column<int>(type: "integer", nullable: false),
                    RatePerSegment = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_sms_send_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sms_send_requests_sms_gateways_GatewayId",
                        column: x => x.GatewayId,
                        principalTable: "sms_gateways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sms_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SendRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    GatewayId = table.Column<Guid>(type: "uuid", nullable: false),
                    GatewayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Recipient = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "text", nullable: false),
                    Encoding = table.Column<int>(type: "integer", nullable: false),
                    Segments = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProviderResponse = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    CostIsActual = table.Column<bool>(type: "boolean", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StatusChecks = table.Column<int>(type: "integer", nullable: false),
                    LastStatusCheckAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_sms_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sms_messages_sms_gateways_GatewayId",
                        column: x => x.GatewayId,
                        principalTable: "sms_gateways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sms_messages_sms_send_requests_SendRequestId",
                        column: x => x.SendRequestId,
                        principalTable: "sms_send_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sms_gateways_CompanyId_Default",
                table: "sms_gateways",
                column: "CompanyId",
                unique: true,
                filter: "\"IsDefault\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_sms_gateways_CompanyId_Name",
                table: "sms_gateways",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_CompanyId_CreatedAt",
                table: "sms_messages",
                columns: new[] { "CompanyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_CompanyId_GatewayId_CreatedAt",
                table: "sms_messages",
                columns: new[] { "CompanyId", "GatewayId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_CompanyId_Status_CreatedAt",
                table: "sms_messages",
                columns: new[] { "CompanyId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_GatewayId",
                table: "sms_messages",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_ProviderMessageId",
                table: "sms_messages",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_Recipient",
                table: "sms_messages",
                column: "Recipient");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_SendRequestId",
                table: "sms_messages",
                column: "SendRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_Status_NextAttemptAt",
                table: "sms_messages",
                columns: new[] { "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_Status_SubmittedAt",
                table: "sms_messages",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_send_requests_CompanyId_CreatedAt",
                table: "sms_send_requests",
                columns: new[] { "CompanyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_send_requests_CompanyId_IdempotencyKey",
                table: "sms_send_requests",
                columns: new[] { "CompanyId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sms_send_requests_GatewayId",
                table: "sms_send_requests",
                column: "GatewayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_messages");

            migrationBuilder.DropTable(
                name: "sms_send_requests");

            migrationBuilder.DropTable(
                name: "sms_gateways");
        }
    }
}
