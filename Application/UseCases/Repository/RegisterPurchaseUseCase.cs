using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class RegisterPurchaseUseCase : IRegisterPurchaseUseCase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IAppDbContext _context;

        public RegisterPurchaseUseCase(
            IInventoryService inventoryService,
            IPurchaseRepository purchaseRepository,
            IAppDbContext context)
        {
            _inventoryService = inventoryService;
            _purchaseRepository = purchaseRepository;
            _context = context;
        }

        public async Task ExecuteAsync(
            Guid businessId,
            IReadOnlyList<(Guid VariantId, int Quantity, decimal Cost)> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items must contain at least one line.", nameof(items));

            foreach (var line in items)
            {
                if (line.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero for each line.", nameof(items));

                if (line.Cost < 0)
                    throw new ArgumentException("Cost must be greater than or equal to zero for each line.", nameof(items));
            }

            var purchaseId = Guid.NewGuid();
            var purchaseDate = DateTime.UtcNow;
            var total = items.Sum(i => i.Quantity * i.Cost);

            var purchase = new Purchase
            {
                Id = purchaseId,
                BusinessId = businessId,
                PurchaseDate = purchaseDate,
                Total = total,
                Items = items
                    .Select(
                        line => new PurchaseItem
                        {
                            Id = Guid.NewGuid(),
                            PurchaseId = purchaseId,
                            CatalogVariantId = line.VariantId,
                            Quantity = line.Quantity,
                            Cost = line.Cost,
                        })
                    .ToList(),
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var line in items)
                {
                    await _inventoryService.IncreaseStockAsync(
                        businessId,
                        line.VariantId,
                        line.Quantity,
                        "Purchase");
                }

                await _purchaseRepository.AddPurchaseAsync(purchase);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
