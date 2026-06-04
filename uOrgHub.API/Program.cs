using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using uOrgHub.Accounts;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth;
using uOrgHub.HR;
using uOrgHub.Inventory;
using uOrgHub.Procurement;
using uOrgHub.Projects;
using uOrgHub.API.Services;
using uOrgHub.Shared.Data;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

// Dashboard
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Cross-module services
builder.Services.AddScoped<IEmployeeWithUserService, EmployeeWithUserService>();

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

// Dashboard Service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(t => t.FullName?.Replace("+", "_") ?? t.Name);
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var configured = builder.Configuration["AllowedOrigins"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        if (configured.Length > 0)
            policy.WithOrigins(configured).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        else
            policy.SetIsOriginAllowed(o => new Uri(o).Host == "localhost")
                  .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
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

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

Console.WriteLine("=== uOrgHub API STARTED ===");

app.Run();
