using OrgHub.Api.Extensions;
using OrgHub.Api.MiddleWares;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Mapping;
using OrgHub.Domain.Entities;
using OrgHub.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Centralize Serilog configuration
builder.Host.AddSerilogLogging();

// Register Controllers
builder.Services.AddControllers();

// Register Swagger
builder.Services.AddSwaggerDocumentation();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateEmployeeCommand>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetByIdCommand>());

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(EmployeeProfile).Assembly);

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

app.UseAuthorization();

//Register Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Route Controllers
app.MapControllers();

app.Run();
