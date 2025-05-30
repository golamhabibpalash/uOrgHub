using OrgHub.Api.Extensions;
using OrgHub.Api.MiddleWares;
using OrgHub.Infrastructure.DependencyInjection;
using OrgHub.Infrastructure.Persistence.Others;

var builder = WebApplication.CreateBuilder(args);

// Centralize logging configuration
//builder.Host.AddSerilogLogging(builder.Configuration);
//builder.Services.AddCustomLogging();
builder.Services.AddHttpContextAccessor(); 
//builder.Services.AddSingleton<AuditLoggingInterceptor>();


// Register Controllers
builder.Services.AddControllers();


// Register Swagger
builder.Services.AddSwaggerDocumentation();

// Register MediatR
builder.Services.AddMediatRServices();

// Register AutoMapper
builder.Services.AddAutoMapperProfiles();

// Initiate DI
builder.Services.AddInfrastructure(builder.Configuration);

//Register Authentication/Authorization/JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/Identity/swagger.json", "Identity APIs");
        c.SwaggerEndpoint("/swagger/HRM/swagger.json", "HRM APIs");
        c.SwaggerEndpoint("/swagger/FixedAssets/swagger.json", "FixedAssets APIs");
        c.SwaggerEndpoint("/swagger/Inventory/swagger.json", "Inventory APIs");
        c.SwaggerEndpoint("/swagger/ProjectManagement/swagger.json", "Project Management APIs");

        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrgHub API v1");
    });
}

app.UseHttpsRedirection();

//Register Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseAuthentication();
app.UseAuthorization();

// Route Controllers
app.MapControllers();

app.Run();
