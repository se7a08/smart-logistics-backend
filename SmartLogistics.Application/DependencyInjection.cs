using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartLogistics.Application.Common.Behaviors;
using SmartLogistics.Application.Common.Mappings;
using SmartLogistics.Application.Common.Validators;


namespace SmartLogistics.Application
{
    public static class DependencyInjection
    {
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
    }
}
