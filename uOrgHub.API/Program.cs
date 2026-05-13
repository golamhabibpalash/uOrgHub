using Microsoft.EntityFrameworkCore;
using uOrgHub.HR;
using uOrgHub.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HR Module
builder.Services.AddHRModule();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<uOrgHub.API.Middleware.ExceptionMiddleware>();

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
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("=== uOrgHub API STARTED ===");

app.Run();