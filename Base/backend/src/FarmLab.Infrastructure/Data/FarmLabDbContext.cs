// FarmLab.Infrastructure/Data/FarmLabDbContext.cs
using Microsoft.EntityFrameworkCore;
using FarmLab.Domain.Entities;

namespace FarmLab.Infrastructure.Data
{
    public class FarmLabDbContext : DbContext
    {
        public FarmLabDbContext(DbContextOptions<FarmLabDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Talhao> Talhoes { get; set; }
        public DbSet<SoilSample> SoilSamples { get; set; }
        public DbSet<WeatherRecord> WeatherRecords { get; set; }
        public DbSet<NdviReading> NdviReadings { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<PestRecord> PestRecords { get; set; }
        public DbSet<ActionPlan> ActionPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Simple configurations – primary keys are inferred
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            // Add any required indexes or relationships later
            base.OnModelCreating(modelBuilder);
        }
    }
}
