using GeoMac.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GeoMac.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
