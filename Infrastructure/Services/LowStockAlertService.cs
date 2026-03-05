using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class LowStockAlertService : ILowStockAlertService
    {
        public Task NotifyLowStock(Guid businessId, CatalogVariant variant)
        {
            Console.WriteLine($"Low stock alert: {variant.SKU}");

            return Task.CompletedTask;
        }
    }
}
