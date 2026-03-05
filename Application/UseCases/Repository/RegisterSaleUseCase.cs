using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class RegisterSaleUseCase
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;

        public RegisterSaleUseCase(
            ISaleRepository saleRepository,
            IInventoryService inventoryService)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
        }

        public async Task<Guid> ExecuteAsync(
            Guid businessId,
            List<(Guid variantId, int quantity, decimal price)> items)
        {
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                SaleDate = DateTime.UtcNow
            };

            foreach (var item in items)
            {
                sale.Items.Add(new SaleItem
                {
                    Id = Guid.NewGuid(),
                    CatalogVariantId = item.variantId,
                    Quantity = item.quantity,
                    Price = item.price
                });

                await _inventoryService.DecreaseStockAsync(
                    businessId,
                    item.variantId,
                    item.quantity,
                    "Sale"
                );
            }

            sale.Total = sale.Items.Sum(x => x.Price * x.Quantity);

            await _saleRepository.AddSaleAsync(sale);

            return sale.Id;
        }
    }
}
