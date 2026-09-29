using GeoMac.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeoMac.Infrastructure;

public class GeoMacDbContext : IdentityDbContext<ApplicationUser>
{
    public GeoMacDbContext(DbContextOptions<GeoMacDbContext> options)
        : base(options) { }

    public DbSet<CropAnalysis> CropAnalyses { get; set; } = null!;
    public DbSet<WeatherMetric> WeatherMetrics { get; set; } = null!;
    public DbSet<SoilMetric> SoilMetrics { get; set; } = null!;
    public DbSet<NdviReading> NdviReadings { get; set; } = null!;
    public DbSet<ActionPlan> ActionPlans { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configurações adicionais se necessário
    }
}
