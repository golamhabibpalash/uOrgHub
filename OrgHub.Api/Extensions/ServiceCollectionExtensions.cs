using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.Identity.Commands;
using OrgHub.Application.Mapping;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Reflection;
using System.Text;

namespace OrgHub.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring services in the OrgHub API.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Serilog logging for the application.
    /// </summary>
    /// <param name="hostBuilder">The host builder to which Serilog logging will be added.</param>
    /// <param name="configuration">The application configuration containing logging settings.</param>
    /// <returns>The updated host builder.</returns>
    public static IHostBuilder AddSerilogLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day)
            .WriteTo.MSSqlServer(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                },
                restrictedToMinimumLevel: LogEventLevel.Information
                // columnOptions: columnOptions // optionally include if you're customizing columns
            )
            .CreateLogger();

        hostBuilder.UseSerilog();
        return hostBuilder;
    }

    /// <summary>
    /// Adds JWT authentication services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which authentication services will be added.</param>
    /// <param name="configuration">The application configuration containing JWT settings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                //ValidIssuer = configuration["Jwt:Issuer"],
                //ValidAudience = configuration["Jwt:Audience"],
            };
        });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds Swagger documentation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which Swagger services will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("Identity", new OpenApiInfo { Title = "Identity APIs", Version = "v1", Description = "API Documentation for Authentication/Authorization/Claim/Role and so on" });
            options.SwaggerDoc("HRM", new OpenApiInfo { Title = "HRM APIs", Version = "v1", Description = "API Documentation for Human Resource Management" });
            options.SwaggerDoc("FixedAssets", new OpenApiInfo { Title = "FixedAssets APIs", Version = "v1", Description = "Assets Management Systems API" });
            options.SwaggerDoc("Inventory", new OpenApiInfo { Title = "Inventory APIs", Version = "v1", Description = "Inventory Management Systems API" });
            options.SwaggerDoc("ProjectManagement", new OpenApiInfo { Title = "Project Management APIs", Version = "v1", Description = "Project Management Systems API" });

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                var area = apiDesc.ActionDescriptor?.RouteValues["area"];
                return string.Equals(docName, area, StringComparison.OrdinalIgnoreCase);
            });


            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Adds MediatR services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which MediatR services will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMediatRServices(this IServiceCollection services)
    {
        // Register all MediatR handlers from relevant assemblies
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateEmployeeCommand>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateEquipmentCommand>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AssignPermissionToRoleCommand>());
        return services;
    }

    /// <summary>
    /// Adds AutoMapper profiles to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which AutoMapper profiles will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(EmployeeProfile).Assembly);
        services.AddAutoMapper(typeof(EquipmentProfile).Assembly);
        return services;
    }
}