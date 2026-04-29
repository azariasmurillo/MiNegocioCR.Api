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
        public DbSet<RepairOrderItem> RepairOrderItems => Set<RepairOrderItem>();
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
        public DbSet<CatalogVariantOptionValue> CatalogVariantOptionValues => Set<CatalogVariantOptionValue>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();
        public DbSet<ConversationState> ConversationStates => Set<ConversationState>();
        public DbSet<AITokenUsage> AITokenUsages => Set<AITokenUsage>();
        public DbSet<UpsellRule> UpsellRules => Set<UpsellRule>();
        public DbSet<QuickReplyTemplate> QuickReplyTemplates { get; set; }
        public DbSet<ConversationTag> ConversationTags { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessSettings>()
                .HasKey(x => x.BusinessId);

            modelBuilder.Entity<Business>(entity =>
            {
                entity.Property(x => x.LogoUrl).HasMaxLength(500);
                entity.Property(x => x.BusinessType).HasMaxLength(120);
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.Phone).HasMaxLength(50);
                entity.Property(x => x.Location).HasMaxLength(250);
                entity.Property(x => x.PublicEmail).HasMaxLength(150);
            });

            modelBuilder.Entity<User>()
                .HasIndex(x => x.FirebaseUid)
                .IsUnique();

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(x => x.MessageId)
                .IsUnique();

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(x => new { x.ConversationId, x.Timestamp });

            modelBuilder.Entity<WhatsAppConversation>()
                .HasIndex(x => new { x.BusinessId, x.PhoneNumber })
                .IsUnique();

            modelBuilder.Entity<WhatsAppConversation>()
                .HasOne(x => x.Business)
                .WithMany(x => x.WhatsAppConversations)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WhatsAppMessage>()
                .HasOne(x => x.Conversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WhatsAppConversation>()
                .HasIndex(x => x.LastMessageAt);

            modelBuilder.Entity<WhatsAppConversation>()
                .HasIndex(x => x.IsArchived);

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

            // Variante ↔ valores de opción (combinación; tabla de unión explícita con Id).
            modelBuilder.Entity<CatalogVariantOptionValue>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.CatalogVariant)
                    .WithMany(v => v.VariantOptionValues)
                    .HasForeignKey(x => x.CatalogVariantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CatalogOptionValue)
                    .WithMany(v => v.VariantOptionValues)
                    .HasForeignKey(x => x.CatalogOptionValueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.CatalogVariantId, x.CatalogOptionValueId })
                    .IsUnique();
            });

            modelBuilder.Entity<Purchase>()
                .HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .IsRequired(false)
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

            modelBuilder.Entity<SaleItem>()
                .HasOne<CatalogVariant>()
                .WithMany()
                .HasForeignKey(x => x.CatalogVariantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Contact)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(x => x.InvoiceNumber)
                    .IsRequired()
                    .HasMaxLength(32);
                entity.Property(x => x.HaciendaConsecutive)
                    .HasMaxLength(50);
                entity.Property(x => x.Source)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(x => x.Subtotal)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.Tax)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.Discount)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.Total)
                    .HasColumnType("numeric(18,2)");
                entity.HasIndex(x => new { x.BusinessId, x.InvoiceNumber })
                    .IsUnique();
            });

            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.Property(x => x.ItemType)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Sale>()
                .HasIndex(x => x.ContactId);

            modelBuilder.Entity<ConversationTag>()
                .HasOne(x => x.Conversation)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contact>()
                .HasIndex(x => new { x.BusinessId, x.Phone })
                .IsUnique();

            modelBuilder.Entity<Contact>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<RepairOrder>(entity =>
            {
                entity.HasOne(o => o.Contact)
                    .WithMany(c => c.RepairOrders)
                    .HasForeignKey(o => o.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.OrderNumber)
                    .IsRequired()
                    .HasMaxLength(32);
                entity.Property(x => x.DeviceType).HasMaxLength(50);
                entity.Property(x => x.DeviceTypeOther).HasMaxLength(200);
                entity.Property(x => x.Brand).HasMaxLength(100);
                entity.Property(x => x.Model).HasMaxLength(150);
                entity.Property(x => x.SerialNumber).HasMaxLength(150);
                entity.Property(x => x.AccessoriesIncluded).HasMaxLength(500);
                entity.Property(x => x.OperatingSystem).HasMaxLength(100);
                entity.Property(x => x.Password).HasMaxLength(200);

                entity.Property(x => x.DiscountPercent)
                    .HasColumnType("numeric(18,2)");

                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => x.ContactId);
                entity.HasIndex(x => new { x.BusinessId, x.CreatedAt });
                entity.HasIndex(x => new { x.BusinessId, x.OrderNumber })
                    .IsUnique();

                entity.HasIndex(x => new { x.BusinessId, x.Status });
            });

            modelBuilder.Entity<RepairOrderItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.RepairOrder)
                    .WithMany(o => o.Items)
                    .HasForeignKey(x => x.RepairOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CatalogVariant)
                    .WithMany(v => v.RepairOrderItems)
                    .HasForeignKey(x => x.CatalogVariantId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.Price)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.Quantity);

                entity.HasIndex(x => x.RepairOrderId);
                entity.HasIndex(x => x.CatalogVariantId);
            });

            modelBuilder.Entity<ConversationTag>()
                .HasIndex(x => new { x.ConversationId, x.Tag });

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

            modelBuilder.Entity<QuickReplyTemplate>()
                .HasIndex(x => new { x.BusinessId, x.Name });
        }
    }
}