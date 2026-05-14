using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using uOrgHub.Accounts;
using uOrgHub.API.Middleware;
using uOrgHub.Auth;
using uOrgHub.HR;
using uOrgHub.Inventory;
using uOrgHub.Procurement;
using uOrgHub.Projects;
using uOrgHub.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
    });

// Memory cache for permission caching
builder.Services.AddMemoryCache();

// HR Module
builder.Services.AddHRModule();

// Accounts Module
builder.Services.AddAccountsModule();

// Inventory Module
builder.Services.AddInventoryModule();

// Procurement Module
builder.Services.AddProcurementModule();

// Projects Module
builder.Services.AddProjectsModule();

// Auth Module
builder.Services.AddAuthModule();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Middleware order (MUST be exact):
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AccessLogMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PermissionMiddleware>();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration skipped: {Message}", ex.Message);
        Console.WriteLine($"Database migration skipped: {ex.Message}\nThe application will continue without applying migrations.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

Console.WriteLine("=== uOrgHub API STARTED ===");

app.Run();
