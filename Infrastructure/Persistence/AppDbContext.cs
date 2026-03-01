using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Business> Businesses => Set<Business>();
        public DbSet<User> Users => Set<User>();
        public DbSet<BusinessSettings> BusinessSettings => Set<BusinessSettings>();
        public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessSettings>()
                .HasKey(x => x.BusinessId);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.FirebaseUid)
                .IsUnique();

            modelBuilder.Entity<RepairOrder>()
                .HasIndex(x => new { x.BusinessId, x.OrderNumber })
                .IsUnique();
        }
    }
}