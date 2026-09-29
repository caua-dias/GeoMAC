using GeoMac.Domain.Entities;
using GeoMac.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
