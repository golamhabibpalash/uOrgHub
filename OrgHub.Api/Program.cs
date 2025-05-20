using OrgHub.Api.Extensions;
using OrgHub.Api.MiddleWares;
using OrgHub.Application.Mapping;
using OrgHub.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Centralize Serilog configuration
builder.Host.AddSerilogLogging();

// Register Controllers
builder.Services.AddControllers();

//Register Authentication/Authorization/JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register Swagger
builder.Services.AddSwaggerDocumentation();

// Register MediatR
builder.Services.AddMediatRServices();

// Register AutoMapper
builder.Services.AddAutoMapperProfiles();

// Initiate DI
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrgHub API v1");
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
