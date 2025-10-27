using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Infrastructure.Persistence;
using AugmentedScribe.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AugmentedScribe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresDb");
        services.AddDbContext<ScribeDbContext>(options =>
            options.UseNpgsql(connectionString, b => 
                b.MigrationsAssembly("AugmentedScribe.Infrastructure")));
        
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}