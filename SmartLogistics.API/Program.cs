using Serilog;
using SmartLogistics.API.Extensions;
using SmartLogistics.API.Middleware;
using SmartLogistics.Infrastructure.Data;
using SmartLogistics.Infrastructure.Data.Seeding;
using SmartLogistics.Infrastructure.Hubs;
using SmartLogistics.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddApplication(); // Registers MediatR, Automapper, and Validators
builder.Services.AddInfrastructure(builder.Configuration); // Registers DB and Repositories
builder.Services.AddJwtAuthentication(builder.Configuration); // Configures JWT Security
builder.Services.AddSwaggerDocumentation(); // Configures Swagger UI
builder.Services.AddCorsPolicy(builder.Configuration); // Configures CORS for Flutter/Web
builder.Services.AddSignalR(); // Enables real-time communication for tracking

var app = builder.Build();

// Initialize and seed the database during startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DatabaseSeeder.SeedAsync(db, hasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline (Middleware)

// 1. Global Exception Handling should be the first to catch any errors
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Logging Middleware to track all incoming requests
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment()|| app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapHub<TrackingHub>("/hubs/tracking");

app.Run();