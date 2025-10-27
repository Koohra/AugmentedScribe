using AugmentedScribe.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

namespace AugmentedScribe;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(); // (Vamos configurar o Swagger para JWT depois)

        // 3. Adiciona o CORS (Permitindo o React)
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            {
                policy.WithOrigins("http://localhost:3000") // URL React
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // 4. Adiciona Autenticação JWT (Vamos configurar na Fatia 1)
        // services.AddAuthentication(...) 

        return services;
    }
}