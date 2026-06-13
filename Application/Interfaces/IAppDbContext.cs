using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using ConversationtagEntity = MiNegocioCR.Api.Domain.Entities.ConversationTag;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MiNegocioCR.Api.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<BusinessEntity> Businesses { get; }
        DbSet<RepairOrder> RepairOrders { get; }
        DbSet<RepairOrderItem> RepairOrderItems { get; }
        DbSet<User> Users { get; }
        DbSet<PasswordResetToken> PasswordResetTokens { get; }
        DbSet<BusinessSettings> BusinessSettings { get; }
        DbSet<WhatsAppMessage> WhatsAppMessages { get; }
        DbSet<WhatsAppConversation> WhatsAppConversations { get; }
        DbSet<CatalogItem> CatalogItems { get; }
        DbSet<CatalogVariant> CatalogVariants { get; }
        DbSet<InventoryMovement> InventoryMovements { get; }
        DatabaseFacade Database { get; }
        DbSet<UpsellRule> UpsellRules { get; }
        DbSet<QuickReplyTemplate> QuickReplyTemplates { get; }
        DbSet <ConversationtagEntity> ConversationTags { get; }
        DbSet<ContactEntity> Contacts { get; }
        DbSet<Sale> Sales { get; }
        DbSet<SaleItem> SaleItems { get; }
        DbSet<SalePaymentMethod> SalePaymentMethods { get; }
        DbSet<Payment> Payments { get; }
        DbSet<RepairOrderImage> RepairOrderImages { get; }
        DbSet<CatalogVariantImage> CatalogVariantImages { get; }
        DbSet<ContactEmailCampaignLog> ContactEmailCampaignLogs { get; }
        DbSet<EmailCampaign> EmailCampaigns { get; }
        DbSet<EmailCampaignRecipient> EmailCampaignRecipients { get; }
        DbSet<InternetOrder> InternetOrders { get; }
        DbSet<InternetOrderLine> InternetOrderLines { get; }
        DbSet<InternetOrderAdvance> InternetOrderAdvances { get; }
        DbSet<CreditAccount> CreditAccounts { get; }
        DbSet<CreditTransaction> CreditTransactions { get; }
        DbSet<CreditTransactionLine> CreditTransactionLines { get; }
        DbSet<CreditCommunication> CreditCommunications { get; }
        DbSet<ImageImportBatch> ImageImportBatches { get; }
        DbSet<ImageImportLog> ImageImportLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
