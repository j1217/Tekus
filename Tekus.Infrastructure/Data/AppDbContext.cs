// Tekus.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using Tekus.Core.Domain;

namespace Tekus.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Provider> Providers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de relaciones muchos-a-muchos (Services <-> Countries)
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Countries)
                .WithMany();

            // Configuración de campos personalizados (Value Objects)
            modelBuilder.Entity<Provider>()
                .OwnsMany(p => p.CustomFields);
        }
    }
}