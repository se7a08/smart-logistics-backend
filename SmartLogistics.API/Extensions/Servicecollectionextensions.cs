using FluentValidation;
using SmartLogistics.Application.Common.Mappings;
using SmartLogistics.Application.Common.Validators;
using SmartLogistics.Domain.Interfaces;
using SmartLogistics.Infrastructure.Data;
using SmartLogistics.Infrastructure.Repositories;
using SmartLogistics.Infrastructure.Services.Auth;
using SmartLogistics.Infrastructure.Services.Notifications;
using SmartLogistics.Infrastructure.Services.QRCode;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartLogistics.Application.Common.Behaviors;
using System.Text;
using SmartLogistics.Infrastructure.Hubs;
using Microsoft.Extensions.Options;

namespace SmartLogistics.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly("SmartLogistics.Infrastructure")
                              .EnableRetryOnFailure(3)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IQrCodeService, QrCodeService>();
            services.AddScoped<INotificationService, FcmNotificationService>();
            services.AddScoped<ITrackingService, TrackingService>(); 

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(MappingProfile).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            });

            
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? "SmartLogisticsSuperSecretKey2026");

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
            });

            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Smart Logistics API",
                    Version = "v1",
                    Description = "Shipment Management and Logistics System"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter token here as: Bearer {token}",
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
            });

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());


                options.AddPolicy("Production", policy =>
                {
                    var origins = configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? [];
                    

                    policy
                        .WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                   
                });
            });


                return services;
        }
    }
}