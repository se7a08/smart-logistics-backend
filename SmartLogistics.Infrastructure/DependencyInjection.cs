using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLogistics.Domain.Interfaces;
using SmartLogistics.Infrastructure.Data;
using SmartLogistics.Infrastructure.Hubs;
using SmartLogistics.Infrastructure.Repositories;
using SmartLogistics.Infrastructure.Services.Auth;
using SmartLogistics.Infrastructure.Services.Notifications;
using SmartLogistics.Infrastructure.Services.QRCode;


namespace SmartLogistics.Infrastructure
{
    public static class DependencyInjection
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



    }
}
