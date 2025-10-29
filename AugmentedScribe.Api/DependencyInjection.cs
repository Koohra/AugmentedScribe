using System.Text;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Infrastructure.Persistence;
using AugmentedScribe.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AugmentedScribe;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddScalarConfiguration();
        services.AddIdentityConfiguration();
        services.AddJwtAuthentication(configuration);
        services.AddCorsConfiguration(configuration);
        services.AddCurrentUserService();

        return services;
    }

    private static IServiceCollection AddScalarConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization header using the Bearer scheme.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            options.AddDocumentTransformer(async (document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes.TryAdd("Bearer", securityScheme);

                document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                await Task.CompletedTask;
            });
        });

        return services;
    }

    private static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ScribeDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecret = configuration["JwtSettings:Secret"]
                        ?? throw new InvalidOperationException(
                            "JWT Secret is not configured in appsettings.Development.json");

        var jwtIssuer = configuration["JwtSettings:Issuer"]
                        ?? throw new InvalidOperationException(
                            "JWT Issuer is not configured in appsettings.Development.json");

        var jwtAudience = configuration["JwtSettings:Audience"]
                          ?? throw new InvalidOperationException(
                              "JWT Audience is not configured in appsettings.Development.json");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        return services;
    }

    private static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            {
                var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
                                     ?? ["http://localhost:3000"];

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}