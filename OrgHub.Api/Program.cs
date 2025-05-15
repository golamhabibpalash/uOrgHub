using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Register Controllers
builder.Services.AddControllers();

// Enable Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MediatR
builder.Services.AddMediatR(Assembly.Load("OrgHub.Application"));

var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // This is the key part that defines the OpenAPI JSON endpoint
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrgHub API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Route Controllers
app.MapControllers();

app.Run();
