using FluentValidation;
using Identity.Application.Common.Behaviors;
using Identity.Application.Common.Mappings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.AddMediatR(config => 
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(typeof(MappingProfile));
        
        return services;
    }
}