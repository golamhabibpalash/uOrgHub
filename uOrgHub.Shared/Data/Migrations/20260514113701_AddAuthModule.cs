using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace uOrgHub.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auth_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_auth_claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "auth_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_auth_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "auth_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsTwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "None"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsLockedOut = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    ProfilePicture = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_auth_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "auth_role_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_auth_role_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_role_claims_auth_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "auth_claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_auth_role_claims_auth_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "auth_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_access_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_access_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_access_logs_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "auth_refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_refresh_tokens_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_two_factor_otps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OTPCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OTPType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SentTo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_two_factor_otps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_two_factor_otps_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_user_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_auth_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_user_claims_auth_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "auth_claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_auth_user_claims_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_user_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_auth_user_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_user_roles_auth_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "auth_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_auth_user_roles_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_user_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeviceInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Browser = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Os = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogoutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LogoutReason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_user_sessions_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auth_access_logs_CreatedAt",
                table: "auth_access_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_auth_access_logs_UserId_CreatedAt",
                table: "auth_access_logs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_auth_claims_Name",
                table: "auth_claims",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_auth_refresh_tokens_Token",
                table: "auth_refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_refresh_tokens_UserId",
                table: "auth_refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_auth_role_claims_ClaimId",
                table: "auth_role_claims",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_auth_role_claims_RoleId_ClaimId",
                table: "auth_role_claims",
                columns: new[] { "RoleId", "ClaimId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_roles_Name",
                table: "auth_roles",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_auth_two_factor_otps_UserId_OTPType_IsUsed",
                table: "auth_two_factor_otps",
                columns: new[] { "UserId", "OTPType", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_auth_user_claims_ClaimId",
                table: "auth_user_claims",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_auth_user_claims_UserId_ClaimId",
                table: "auth_user_claims",
                columns: new[] { "UserId", "ClaimId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_user_roles_RoleId",
                table: "auth_user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_auth_user_roles_UserId_RoleId",
                table: "auth_user_roles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_user_sessions_SessionToken",
                table: "auth_user_sessions",
                column: "SessionToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_user_sessions_UserId_IsActive",
                table: "auth_user_sessions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_auth_users_Email",
                table: "auth_users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_auth_users_Username",
                table: "auth_users",
                column: "Username",
                unique: true,
                filter: "\"IsDeleted\" = false");

            // ── SEED DATA ────────────────────────────────────────────────────────────

            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var sysBy = "system";

            // Roles
            var adminRoleId = Guid.Parse("11111111-0000-0000-0000-000000000001");
            var pmRoleId    = Guid.Parse("11111111-0000-0000-0000-000000000002");
            var seRoleId    = Guid.Parse("11111111-0000-0000-0000-000000000003");
            var acRoleId    = Guid.Parse("11111111-0000-0000-0000-000000000004");
            var skRoleId    = Guid.Parse("11111111-0000-0000-0000-000000000005");
            var hrRoleId    = Guid.Parse("11111111-0000-0000-0000-000000000006");
            var poRoleId    = Guid.Parse("11111111-0000-0000-0000-000000000007");

            migrationBuilder.InsertData("auth_roles", new[] { "Id","Name","Description","IsSystem","IsActive","CreatedAt","CreatedBy","IsDeleted" }, new object[,]
            {
                { adminRoleId, "Admin",               "Full system access",             true, true, now, sysBy, false },
                { pmRoleId,    "ProjectManager",      "Project management access",      true, true, now, sysBy, false },
                { seRoleId,    "SiteEngineer",        "Site-level project access",      true, true, now, sysBy, false },
                { acRoleId,    "Accountant",          "Accounting and finance access",  true, true, now, sysBy, false },
                { skRoleId,    "StoreKeeper",         "Inventory management access",    true, true, now, sysBy, false },
                { hrRoleId,    "HRManager",           "HR management access",           true, true, now, sysBy, false },
                { poRoleId,    "ProcurementOfficer",  "Procurement management access",  true, true, now, sysBy, false },
            });

            // Claims
            static Guid C(int n) => Guid.Parse($"22222222-0000-0000-0000-{n:D12}");
            var claimDefs = new (Guid Id, string Name, string Module, string Category)[]
            {
                (C(1),  "HR.View",                  "HR",          "View"),
                (C(2),  "HR.Create",                "HR",          "Create"),
                (C(3),  "HR.Edit",                  "HR",          "Edit"),
                (C(4),  "HR.Delete",                "HR",          "Delete"),
                (C(5),  "HR.Payroll",               "HR",          "Payroll"),
                (C(6),  "Inventory.View",            "Inventory",   "View"),
                (C(7),  "Inventory.Create",          "Inventory",   "Create"),
                (C(8),  "Inventory.Edit",            "Inventory",   "Edit"),
                (C(9),  "Inventory.Delete",          "Inventory",   "Delete"),
                (C(10), "Inventory.Transactions",    "Inventory",   "Transactions"),
                (C(11), "Accounts.View",             "Accounts",    "View"),
                (C(12), "Accounts.Create",           "Accounts",    "Create"),
                (C(13), "Accounts.Edit",             "Accounts",    "Edit"),
                (C(14), "Accounts.Delete",           "Accounts",    "Delete"),
                (C(15), "Accounts.PostJournal",      "Accounts",    "PostJournal"),
                (C(16), "Procurement.View",          "Procurement", "View"),
                (C(17), "Procurement.Create",        "Procurement", "Create"),
                (C(18), "Procurement.Edit",          "Procurement", "Edit"),
                (C(19), "Procurement.Delete",        "Procurement", "Delete"),
                (C(20), "Procurement.ApprovePR",     "Procurement", "Approve"),
                (C(21), "Procurement.ApprovePO",     "Procurement", "Approve"),
                (C(22), "Procurement.ConfirmGRN",    "Procurement", "Confirm"),
                (C(23), "Projects.View",             "Projects",    "View"),
                (C(24), "Projects.Create",           "Projects",    "Create"),
                (C(25), "Projects.Edit",             "Projects",    "Edit"),
                (C(26), "Projects.Delete",           "Projects",    "Delete"),
                (C(27), "Projects.ApproveDPR",       "Projects",    "Approve"),
                (C(28), "Projects.ApproveBOQ",       "Projects",    "Approve"),
                (C(29), "Projects.ApproveExpense",   "Projects",    "Approve"),
                (C(30), "Users.View",                "Users",       "View"),
                (C(31), "Users.Create",              "Users",       "Create"),
                (C(32), "Users.Edit",                "Users",       "Edit"),
                (C(33), "Users.Delete",              "Users",       "Delete"),
                (C(34), "Users.AssignRoles",         "Users",       "AssignRoles"),
                (C(35), "Reports.View",              "Reports",     "View"),
                (C(36), "Reports.Export",            "Reports",     "Export"),
            };

            var claimRows = new object[claimDefs.Length, 8];
            for (int i = 0; i < claimDefs.Length; i++)
            {
                claimRows[i, 0] = claimDefs[i].Id;
                claimRows[i, 1] = claimDefs[i].Name;
                claimRows[i, 2] = DBNull.Value;
                claimRows[i, 3] = claimDefs[i].Module;
                claimRows[i, 4] = claimDefs[i].Category;
                claimRows[i, 5] = true;
                claimRows[i, 6] = now;
                claimRows[i, 7] = false;
            }
            migrationBuilder.InsertData("auth_claims", new[] { "Id","Name","Description","Module","Category","IsActive","CreatedAt","IsDeleted" }, claimRows);

            // Role-claim: Admin → all
            var rcRows = new List<object[]>();
            foreach (var cd in claimDefs)
                rcRows.Add(new object[] { Guid.NewGuid(), adminRoleId, cd.Id, now, sysBy, false });

            // ProjectManager
            foreach (var name in new[] { "Projects.View","Projects.Create","Projects.Edit","Projects.Delete","Projects.ApproveDPR","Projects.ApproveBOQ","Projects.ApproveExpense",
                                          "Procurement.View","Procurement.ApprovePR","Inventory.View","Inventory.Transactions","HR.View" })
                rcRows.Add(new object[] { Guid.NewGuid(), pmRoleId, claimDefs.First(c => c.Name == name).Id, now, sysBy, false });

            // SiteEngineer
            foreach (var name in new[] { "Projects.View","Projects.Create","Projects.Edit","Inventory.View","Inventory.Transactions" })
                rcRows.Add(new object[] { Guid.NewGuid(), seRoleId, claimDefs.First(c => c.Name == name).Id, now, sysBy, false });

            // Accountant
            foreach (var name in new[] { "Accounts.View","Accounts.Create","Accounts.Edit","Accounts.Delete","Accounts.PostJournal",
                                          "Procurement.View","Procurement.ApprovePO","HR.View","Reports.View","Reports.Export" })
                rcRows.Add(new object[] { Guid.NewGuid(), acRoleId, claimDefs.First(c => c.Name == name).Id, now, sysBy, false });

            // StoreKeeper
            foreach (var name in new[] { "Inventory.View","Inventory.Create","Inventory.Edit","Inventory.Delete","Inventory.Transactions",
                                          "Procurement.View","Procurement.ConfirmGRN" })
                rcRows.Add(new object[] { Guid.NewGuid(), skRoleId, claimDefs.First(c => c.Name == name).Id, now, sysBy, false });

            // HRManager
            foreach (var name in new[] { "HR.View","HR.Create","HR.Edit","HR.Delete","HR.Payroll","Reports.View","Reports.Export" })
                rcRows.Add(new object[] { Guid.NewGuid(), hrRoleId, claimDefs.First(c => c.Name == name).Id, now, sysBy, false });

            // ProcurementOfficer
            foreach (var name in new[] { "Procurement.View","Procurement.Create","Procurement.Edit","Procurement.Delete","Procurement.ApprovePR","Procurement.ApprovePO","Procurement.ConfirmGRN",
                                          "Inventory.View","Inventory.Transactions","Accounts.View" })
                rcRows.Add(new object[] { Guid.NewGuid(), poRoleId, claimDefs.First(c => c.Name == name).Id, now, sysBy, false });

            foreach (var row in rcRows)
                migrationBuilder.InsertData("auth_role_claims", new[] { "Id","RoleId","ClaimId","AssignedAt","AssignedBy","IsDeleted" }, row);

            // Admin user
            var adminUserId = Guid.Parse("33333333-0000-0000-0000-000000000001");
            migrationBuilder.InsertData("auth_users", new[] { "Id","Username","Email","PasswordHash","FirstName","LastName","IsActive","IsTwoFactorEnabled","TwoFactorMethod","FailedLoginAttempts","IsLockedOut","MustChangePassword","CreatedAt","CreatedBy","IsDeleted" },
                new object[] { adminUserId, "admin", "admin@uorghub.com",
                    BCrypt.Net.BCrypt.HashPassword("Admin@1234!", workFactor: 12),
                    "System", "Admin", true, false, "None", 0, false, false, now, sysBy, false });

            // Assign Admin role to admin user
            migrationBuilder.InsertData("auth_user_roles", new[] { "Id","UserId","RoleId","AssignedAt","AssignedBy","IsDeleted" },
                new object[] { Guid.NewGuid(), adminUserId, adminRoleId, now, sysBy, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auth_access_logs");

            migrationBuilder.DropTable(
                name: "auth_refresh_tokens");

            migrationBuilder.DropTable(
                name: "auth_role_claims");

            migrationBuilder.DropTable(
                name: "auth_two_factor_otps");

            migrationBuilder.DropTable(
                name: "auth_user_claims");

            migrationBuilder.DropTable(
                name: "auth_user_roles");

            migrationBuilder.DropTable(
                name: "auth_user_sessions");

            migrationBuilder.DropTable(
                name: "auth_claims");

            migrationBuilder.DropTable(
                name: "auth_roles");

            migrationBuilder.DropTable(
                name: "auth_users");
        }
    }
}
