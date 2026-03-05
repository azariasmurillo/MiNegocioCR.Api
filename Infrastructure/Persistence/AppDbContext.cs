using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
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
        public DbSet<WhatsAppMessage> WhatsAppMessages => Set<WhatsAppMessage>();
        public DbSet<WhatsAppConversation> WhatsAppConversations => Set<WhatsAppConversation>();
        public DbSet<WhatsappWebhookLog> WhatsappWebhookLogs => Set<WhatsappWebhookLog>();
        public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
        public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
        public DbSet<CatalogVariant> CatalogVariants => Set<CatalogVariant>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<CatalogCategory> CatalogCategories => Set<CatalogCategory>();
        public DbSet<CatalogImage> CatalogImages => Set<CatalogImage>();
        public DbSet<CatalogOption> CatalogOptions => Set<CatalogOption>();
        public DbSet<CatalogOptionValue> CatalogOptionValues => Set<CatalogOptionValue>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();

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

            modelBuilder.Entity<CatalogItem>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CatalogVariant>()
                .HasOne(v => v.CatalogItem)
                .WithMany(i => i.Variants)
                .HasForeignKey(v => v.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CatalogImage>()
                .HasOne(x => x.CatalogItem)
                .WithMany(i => i.Images)
                .HasForeignKey(x => x.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CatalogOption>()
                .HasOne(x => x.CatalogItem)
                .WithMany()
                .HasForeignKey(x => x.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CatalogOptionValue>()
                .HasOne(x => x.CatalogOption)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.CatalogOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase>()
                .HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(x => x.Purchase)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(x => x.Variant)
                .WithMany()
                .HasForeignKey(x => x.CatalogVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(x => x.Sale)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CatalogCategory>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<Supplier>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<PurchaseItem>()
                .HasIndex(x => x.PurchaseId);

            modelBuilder.Entity<CatalogItem>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<CatalogItem>()
                .HasIndex(x => new { x.BusinessId, x.CategoryId });

            modelBuilder.Entity<CatalogVariant>()
                .HasIndex(x => x.CatalogItemId);

            modelBuilder.Entity<CatalogVariant>()
                .HasIndex(x => x.SKU);

            modelBuilder.Entity<InventoryMovement>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<InventoryMovement>()
                .HasIndex(x => x.CatalogVariantId);

            modelBuilder.Entity<InventoryMovement>()
                .HasIndex(x => new { x.BusinessId, x.CreatedAt });

            modelBuilder.Entity<Purchase>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<CatalogItem>()
                .HasIndex(x => new { x.BusinessId, x.IsActive });

            modelBuilder.Entity<Sale>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<Sale>()
                .HasIndex(x => x.SaleDate);

            modelBuilder.Entity<SaleItem>()
                .HasIndex(x => x.SaleId);
        }
    }
}