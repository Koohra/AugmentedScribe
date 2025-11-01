using System.Text;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Infrastructure.Messaging.Consumers;
using AugmentedScribe.Infrastructure.Persistence;
using AugmentedScribe.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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
        services.AddMassTransitConfiguration(configuration);
        services.AddCurrentUserService();

        return services;
    }

    private static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMq");
        var host = rabbitMqSettings["Host"] ?? throw new InvalidOperationException("RabbitMq:Host not configured");
        var userName = rabbitMqSettings["UserName"] ?? "guest";
        var password = rabbitMqSettings["Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumers(typeof(BookProcessingConsumer).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, "/", h =>
                {
                    h.Username(userName);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint("book-processing",
                    e => { e.ConfigureConsumer<BookProcessingConsumer>(context); });
            });
        });
        return services;
    }

    private static IServiceCollection AddScalarConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                var securityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Bearer token"
                };

                document.Components.SecuritySchemes["Bearer"] = securityScheme;

                return Task.CompletedTask;
            });
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var hasAuthorize = metadata.Any(m => m is Microsoft.AspNetCore.Authorization.AuthorizeAttribute);
                var hasAllowAnonymous =
                    metadata.Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

                if (!hasAuthorize || hasAllowAnonymous)
                {
                    return Task.CompletedTask;
                }

                var schemeRef = new OpenApiSecuritySchemeReference("Bearer", context.Document);
                var requirement = new OpenApiSecurityRequirement
                {
                    [schemeRef] = []
                };

                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(requirement);

                return Task.CompletedTask;
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