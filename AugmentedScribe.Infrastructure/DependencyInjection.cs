using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Infrastructure.Persistence;
using AugmentedScribe.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
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

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ScribeDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthServices, AuthService>();

        return services;
    }
}