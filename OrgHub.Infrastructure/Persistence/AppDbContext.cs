using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrgHub.Domain.Entities.FixedAssets;
using OrgHub.Domain.Entities.HRM;
using OrgHub.Domain.Entities.Identity;
using OrgHub.Domain.Entities.Others;
using OrgHub.Infrastructure.Persistence.Others;
using System.Security.Claims;

namespace OrgHub.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        private readonly AuditLoggingInterceptor _auditLoggingInterceptor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options, AuditLoggingInterceptor auditLoggingInterceptor, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _auditLoggingInterceptor = auditLoggingInterceptor;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChangesAsync();
            return result;
        }


        private void OnBeforeSaveChanges()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdString) ? null : Guid.Parse(userIdString);

            var entries = ChangeTracker.Entries<IAuditableEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastUpdatedAt = DateTime.UtcNow;
                    entry.Entity.LastUpdatedBy = userId;
                }
            }
        }

        private async Task OnAfterSaveChangesAsync()
        {
            if (_auditLoggingInterceptor.PendingLogs.Any())
            {
                AuditLogs.AddRange(_auditLoggingInterceptor.PendingLogs);
                _auditLoggingInterceptor.PendingLogs.Clear(); // important to avoid duplicates
                await base.SaveChangesAsync(); // now safe
            }
        }







        #region Others
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        #endregion Others

        #region HRM
        public DbSet<HRM_Employee> HRM_Employees { get; set; }
        public DbSet<HRM_Attendance> HRM_Attendance { get; set; }
        #endregion HRM

        #region FixedAssets
        public DbSet<FixedAssets_Equipment> FixedAssets_Equipments { get; set; }
        #endregion FixedAssets



        // Optional: override OnModelCreating if you use Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            #region Identity
            // Rename Identity tables (optional, for clarity)
            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<ApplicationUserRole>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<ApplicationUserClaim>().ToTable("AspNetUserClaims");
            modelBuilder.Entity<ApplicationRoleClaim>().ToTable("AspNetRoleClaims");
            modelBuilder.Entity<ApplicationUserLogin>().ToTable("AspNetUserLogins");
            modelBuilder.Entity<ApplicationUserToken>().ToTable("AspNetUserTokens");

            // If you have custom key configuration for your custom entities, keep them:
            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });

            modelBuilder.Entity<ApplicationRoleClaim>(entity =>
            {
                entity.HasKey(e => e.Id);
            });


            #endregion Identity

            #region HRM
            modelBuilder.Entity<HRM_Employee>(entity =>
            {
                entity.HasKey(e => e.Id);

                // identity / auto-increment
                entity.Property(e => e.Id)
                        .ValueGeneratedOnAdd();

                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            });
            #endregion HRM

            #region FixedAssets
            modelBuilder.Entity<FixedAssets_Equipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                // identity / auto-increment
                entity.Property(e => e.Id)
                        .ValueGeneratedOnAdd();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });
            #endregion FixedAssets

            #region Others
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.TableName).HasMaxLength(128).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(20).IsRequired();
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("NOW()");
            });
            #endregion Others
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(_auditLoggingInterceptor);
        }
    }
}
