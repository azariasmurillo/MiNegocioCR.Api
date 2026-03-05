using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces
{
    public interface ILowStockAlertService
    {
        Task NotifyLowStock(Guid businessId, CatalogVariant variant);
    }
}
