using GeoMac.Application.Configuration;
using GeoMac.Application.Interfaces;
using GeoMac.Domain.Entities;
using GeoMac.Domain.Interfaces;
using GeoMac.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GeoMac.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GeoMacDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=geomac.db"));

        services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<GeoMacDbContext>()
                .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        // Configuração do LM Studio / OpenAI Compatible LLM
        services.Configure<LlmSettings>(configuration.GetSection(LlmSettings.SectionName));

        services.AddHttpClient<ILlmService, LlmService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<LlmSettings>>().Value;
            var baseUrl = settings.BaseUrl.EndsWith('/')
                ? settings.BaseUrl
                : $"{settings.BaseUrl}/";

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 120);
        });

        return services;
    }
}
