using Identity.Application.Interfaces;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        // Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IRoleService, RoleService>();
        
        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        return services;
    }
}