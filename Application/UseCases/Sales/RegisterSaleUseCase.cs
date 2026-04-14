using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Sales
{
    public class RegisterSaleUseCase : IRegisterSaleUseCase
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IAppDbContext _context;

        public RegisterSaleUseCase(
            ISaleRepository saleRepository,
            IInventoryService inventoryService,
            IAppDbContext context)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
            _context = context;
        }

        public async Task<Guid> ExecuteAsync(
            Guid businessId,
            List<(Guid variantId, int quantity, decimal price)> items,
            string? customerPhone = null)
        {
            if (items == null || !items.Any()) throw new ArgumentException("At least one item is required.", nameof(items));

            var phone = customerPhone?.Trim() ?? string.Empty;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sale = new Sale
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    SaleDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CustomerPhone = phone,
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
                        "Sale");
                }

                sale.Total = sale.Items.Sum(x => x.Price * x.Quantity);
                sale.TotalAmount = sale.Total;

                await _saleRepository.AddSaleAsync(sale);

                await transaction.CommitAsync();

                return sale.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
