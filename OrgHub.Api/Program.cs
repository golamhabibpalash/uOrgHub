using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OrgHub.Api.Extensions;
using OrgHub.Api.MiddleWares;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Mapping;
using OrgHub.Infrastructure.DependencyInjection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Centralize Serilog configuration
builder.Host.AddSerilogLogging();

// Register Controllers
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("Challenge: " + context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
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
app.UseAuthentication();
app.UseAuthorization();

//Register Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Route Controllers
app.MapControllers();

app.Run();
