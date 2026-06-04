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
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
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
        public DbSet<SalePaymentMethod> SalePaymentMethods => Set<SalePaymentMethod>();
        public DbSet<ConversationState> ConversationStates => Set<ConversationState>();
        public DbSet<AITokenUsage> AITokenUsages => Set<AITokenUsage>();
        public DbSet<UpsellRule> UpsellRules => Set<UpsellRule>();
        public DbSet<QuickReplyTemplate> QuickReplyTemplates { get; set; }
        public DbSet<ConversationTag> ConversationTags { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<RepairOrderImage> RepairOrderImages => Set<RepairOrderImage>();
        public DbSet<CatalogVariantImage> CatalogVariantImages => Set<CatalogVariantImage>();
        public DbSet<ContactEmailCampaignLog> ContactEmailCampaignLogs => Set<ContactEmailCampaignLog>();
        public DbSet<EmailCampaign> EmailCampaigns => Set<EmailCampaign>();
        public DbSet<EmailCampaignRecipient> EmailCampaignRecipients => Set<EmailCampaignRecipient>();
        public DbSet<InternetOrder> InternetOrders => Set<InternetOrder>();
        public DbSet<InternetOrderLine> InternetOrderLines => Set<InternetOrderLine>();
        public DbSet<InternetOrderAdvance> InternetOrderAdvances => Set<InternetOrderAdvance>();

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
                entity.Property(x => x.DefaultProfitMargin)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0m);
                entity.Property(x => x.TaxRatePercent)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(13m);
            });

            modelBuilder.Entity<CatalogVariant>(entity =>
            {
                entity.Property(x => x.ProfitMargin).HasPrecision(5, 2);
                entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", t =>
                    t.HasCheckConstraint("CK_Users_Role", "\"Role\" IN ('Admin', 'User')"));
                entity.Property(x => x.Email).IsRequired();
                entity.Property(x => x.PasswordHash).IsRequired();
                entity.Property(x => x.FullName).IsRequired(false);
                entity.Property(x => x.Role).IsRequired();
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasIndex(x => x.BusinessId);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("PasswordResetTokens");
                entity.Property(x => x.Token).IsRequired();
                entity.Property(x => x.IsUsed).HasDefaultValue(false);
                entity.HasIndex(x => x.Token).IsUnique();
                entity.HasIndex(x => x.UserId);
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

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

            modelBuilder.Entity<CatalogVariantImage>(entity =>
            {
                entity.ToTable("CatalogVariantImages");
                entity.Property(x => x.ImageUrl).IsRequired();
                entity.Property(x => x.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(x => x.CatalogVariant)
                    .WithMany(v => v.VariantImages)
                    .HasForeignKey(x => x.CatalogVariantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Business)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.BusinessId);
                entity.HasIndex(x => x.CatalogVariantId);
                entity.HasIndex(x => new { x.BusinessId, x.CatalogVariantId });
            });

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
                entity.Property(x => x.TaxAmount)
                    .HasColumnName("Tax")
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.DiscountAmount)
                    .HasColumnName("Discount")
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.DiscountKind)
                    .HasColumnType("smallint")
                    .HasDefaultValue((byte)0);
                entity.Property(x => x.DiscountInputValue)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0m);
                entity.Property(x => x.TotalOrden)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0m);
                entity.Property(x => x.PrepaidAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0m);
                entity.Property(x => x.Total)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.TotalCost)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.TotalProfit)
                    .HasColumnType("numeric(18,2)");
                // TotalAmount es la columna legacy original; se mantiene mapeada
                // para que las queries LINQ del dashboard puedan usarla como fallback.
                // Código nuevo siempre escribe sale.Total; TotalAmount = Total por alias.
                // ValueGeneratedNever: cuando Total=0 EF no debe omitir la columna (Postgres NOT NULL).
                entity.Property(x => x.TotalAmount)
                    .HasColumnType("numeric(18,2)")
                    .ValueGeneratedNever();
                entity.HasIndex(x => new { x.BusinessId, x.InvoiceNumber })
                    .IsUnique();

                entity.HasMany(s => s.PaymentMethods)
                    .WithOne(pm => pm.Sale)
                    .HasForeignKey(pm => pm.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SalePaymentMethod>(entity =>
            {
                entity.Property(x => x.Amount)
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.Method)
                    .IsRequired();
                entity.HasIndex(x => x.SaleId);
            });

            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.Property(x => x.ItemType)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(x => x.CostPrice)
                    .HasColumnType("numeric(18,2)");
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
                entity.Property(x => x.InvoicedAt)
                    .HasColumnType("timestamp with time zone");

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

            modelBuilder.Entity<RepairOrderImage>(entity =>
            {
                entity.Property(x => x.ImageUrl).IsRequired();
                entity.Property(x => x.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(x => x.RepairOrder)
                    .WithMany(o => o.Images)
                    .HasForeignKey(x => x.RepairOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Business)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.BusinessId);
                entity.HasIndex(x => x.RepairOrderId);
                entity.HasIndex(x => new { x.BusinessId, x.RepairOrderId });
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(x => x.Amount)
                    .IsRequired()
                    .HasColumnType("numeric(18,2)");
                entity.Property(x => x.Type)
                    .IsRequired();
                entity.Property(x => x.Method)
                    .IsRequired();
                entity.Property(x => x.Notes)
                    .HasColumnType("text");
                entity.Property(x => x.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(x => x.Business)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RepairOrder)
                    .WithMany()
                    .HasForeignKey(x => x.RepairOrderId)
                    .OnDelete(DeleteBehavior.Restrict);
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
                .HasIndex(x => new { x.BusinessId, x.RepairOrderId })
                .IsUnique();

            modelBuilder.Entity<Sale>()
                .HasIndex(x => x.SaleDate);

            modelBuilder.Entity<SaleItem>()
                .HasIndex(x => x.SaleId);

            modelBuilder.Entity<Payment>()
                .HasIndex(x => x.BusinessId);

            modelBuilder.Entity<Payment>()
                .HasIndex(x => x.RepairOrderId);

            modelBuilder.Entity<Payment>()
                .HasIndex(x => new { x.BusinessId, x.RepairOrderId });

            modelBuilder.Entity<QuickReplyTemplate>()
                .HasIndex(x => new { x.BusinessId, x.Name });

            modelBuilder.Entity<ContactEmailCampaignLog>(entity =>
            {
                entity.Property(x => x.Subject).HasMaxLength(300);
                entity.Property(x => x.Status).HasMaxLength(20);
                entity.Property(x => x.ResendMessageId).HasMaxLength(100);
                entity.Property(x => x.ErrorMessage).HasMaxLength(500);

                entity.HasOne(x => x.Contact)
                    .WithMany()
                    .HasForeignKey(x => x.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Business)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.BusinessId, x.SentAt });
                entity.HasIndex(x => new { x.BusinessId, x.ContactId, x.SentAt });
            });

            modelBuilder.Entity<EmailCampaign>(entity =>
            {
                entity.Property(x => x.SubjectTemplate).HasMaxLength(300);
                entity.Property(x => x.BodyText).HasMaxLength(8000);
                entity.Property(x => x.ImageUrl).HasMaxLength(2000);
                entity.Property(x => x.AudienceMode).HasMaxLength(32);
                entity.Property(x => x.Status).HasMaxLength(20);

                entity.HasOne(x => x.Business)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.BusinessId, x.CreatedAt });
                entity.HasIndex(x => new { x.BusinessId, x.Status });
            });

            modelBuilder.Entity<EmailCampaignRecipient>(entity =>
            {
                entity.Property(x => x.ContactName).HasMaxLength(200);
                entity.Property(x => x.ContactEmail).HasMaxLength(200);
                entity.Property(x => x.Status).HasMaxLength(20);
                entity.Property(x => x.ErrorMessage).HasMaxLength(500);
                entity.Property(x => x.ResendMessageId).HasMaxLength(100);

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.Recipients)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Contact)
                    .WithMany()
                    .HasForeignKey(x => x.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.Status, x.GlobalQueueOrder });
                entity.HasIndex(x => x.CampaignId);
            });

            modelBuilder.Entity<InternetOrder>(entity =>
            {
                entity.Property(x => x.OrderNumber).HasMaxLength(20);
                entity.Property(x => x.ExchangeRateApplied).HasPrecision(18, 4);
                entity.Property(x => x.InternationalShippingCost).HasPrecision(18, 2);
                entity.Property(x => x.LocalCourierCost).HasPrecision(18, 2);
                entity.Property(x => x.ServiceFee).HasPrecision(18, 2);
                entity.Property(x => x.LinesTotalUsd).HasPrecision(18, 2);
                entity.Property(x => x.LinesTotalCrc).HasPrecision(18, 2);
                entity.Property(x => x.SubtotalCrc).HasPrecision(18, 2);
                entity.Property(x => x.TotalAdvancesCrc).HasPrecision(18, 2);
                entity.Property(x => x.BalanceDueCrc).HasPrecision(18, 2);
                entity.Property(x => x.CustomerNotes).HasMaxLength(2000);
                entity.Property(x => x.InternalNotes).HasMaxLength(2000);
                entity.Property(x => x.RefundNote).HasMaxLength(2000);
                entity.Property(x => x.ExternalOrderId).HasMaxLength(100);
                entity.Property(x => x.TrackingNumber).HasMaxLength(200);

                entity.HasOne(x => x.Business)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Contact)
                    .WithMany(c => c.InternetOrders)
                    .HasForeignKey(x => x.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.BusinessId, x.OrderNumber }).IsUnique();
                entity.HasIndex(x => new { x.BusinessId, x.Status, x.CreatedAt });
            });

            modelBuilder.Entity<InternetOrderLine>(entity =>
            {
                entity.Property(x => x.ProductName).HasMaxLength(300);
                entity.Property(x => x.ProductUrl).HasMaxLength(2000);
                entity.Property(x => x.UnitPriceUsd).HasPrecision(18, 2);
                entity.Property(x => x.LineTotalUsd).HasPrecision(18, 2);
                entity.Property(x => x.LineTotalCrc).HasPrecision(18, 2);

                entity.HasOne(x => x.InternetOrder)
                    .WithMany(o => o.Lines)
                    .HasForeignKey(x => x.InternetOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.InternetOrderId);
            });

            modelBuilder.Entity<InternetOrderAdvance>(entity =>
            {
                entity.Property(x => x.AmountCrc).HasPrecision(18, 2);
                entity.Property(x => x.Method).HasMaxLength(50);
                entity.Property(x => x.Notes).HasMaxLength(500);

                entity.HasOne(x => x.InternetOrder)
                    .WithMany(o => o.Advances)
                    .HasForeignKey(x => x.InternetOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.InternetOrderId);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Sale>()
                         .Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                entry.Entity.TotalAmount = entry.Entity.Total;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}