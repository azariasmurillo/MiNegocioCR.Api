using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Migrations;

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
        public DbSet<WhatsAppMessage> WhatsAppMessages => Set<WhatsAppMessage>();
        public DbSet<WhatsAppConversation> WhatsAppConversations { get; set; }
        public DbSet<WhatsappWebhookLog> WhatsappWebhookLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessSettings>()
        .HasKey(x => x.BusinessId);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.FirebaseUid)
                .IsUnique();

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(x => x.MessageId);

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(x => new { x.BusinessId, x.PhoneNumber });

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(x => new { x.BusinessId, x.Timestamp });

            modelBuilder.Entity<WhatsAppConversation>()
                .HasIndex(x => new { x.BusinessId, x.PhoneNumber })
                .IsUnique();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}