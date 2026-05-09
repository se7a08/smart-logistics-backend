using FluentValidation;
using SmartLogistics.Application.Common.Mappings;
using global::SmartLogistics.Application.Common.Validators;
using global::SmartLogistics.Domain.Interfaces;
using global::SmartLogistics.Infrastructure.Data;
using global::SmartLogistics.Infrastructure.Hubs;
using global::SmartLogistics.Infrastructure.Repositories;
using global::SmartLogistics.Infrastructure.Services.Auth;
using global::SmartLogistics.Infrastructure.Services.Background;
using global::SmartLogistics.Infrastructure.Services.Notifications;
using global::SmartLogistics.Infrastructure.Services.QRCode;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartLogistics.Application.Common.Behaviors;

using System.Text;

namespace SmartLogistics.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>Registers all infrastructure services (DB, repositories, services).</summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // EF Core with SQL Server
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly("SmartLogistics.Infrastructure")
                              .EnableRetryOnFailure(3)));

            // Repository Pattern + Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Domain Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IQrCodeService, QrCodeService>();
            services.AddScoped<INotificationService, FcmNotificationService>();
            services.AddScoped<ITrackingService, TrackingService>();

            // Background Services
            services.AddHostedService<DataCleanupService>();

            return services;
        }

        /// <summary>Registers all application-layer services (MediatR, AutoMapper, FluentValidation).</summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR - scans all assemblies containing handlers
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Application.Features.Auth.Commands.RegisterCommand).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            });

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // FluentValidation - auto-discovers validators in application assembly
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            return services;
        }

        /// <summary>Configures JWT Bearer authentication.</summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Allow token via query string for SignalR connections
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            ctx.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }

        /// <summary>Configures Swagger/OpenAPI with JWT support.</summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Smart Logistics API",
                    Version = "v1",
                    Description = "Production-grade logistics and shipment management API with real-time tracking, " +
                                  "QR verification, push notifications, and role-based authentication.",
                    Contact = new OpenApiContact { Name = "Smart Logistics Team", Email = "dev@smartlogistics.com" }
                });

                // JWT Authorization in Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header. Enter: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

                // Include XML comments
                var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        /// <summary>Configures CORS for web and mobile clients.</summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());

                options.AddPolicy("Production", policy =>
                {
                    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                    policy.WithOrigins(origins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
                });
            });

            return services;
        }
    }
}
