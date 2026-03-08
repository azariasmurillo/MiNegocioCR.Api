using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MiNegocioCR.Api.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<BusinessEntity> Businesses { get; }
        DbSet<RepairOrder> RepairOrders { get; }
        DbSet<User> Users { get; }
        DbSet<BusinessSettings> BusinessSettings { get; }
        DbSet<WhatsAppMessage> WhatsAppMessages { get; }
        DbSet<CatalogItem> CatalogItems { get; }
        DbSet<InventoryMovement> InventoryMovements { get; }
        DatabaseFacade Database { get; }
        DbSet<UpsellRule> UpsellRules { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
