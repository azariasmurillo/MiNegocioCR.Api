using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class AdjustInventoryUseCase : IAdjustInventoryUseCase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IAppDbContext _context;

        public AdjustInventoryUseCase(IInventoryService inventoryService, IAppDbContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }

        public async Task ExecuteAsync(
            Guid businessId,
            Guid variantId,
            int adjustment,
            string reason)
        {
            if (adjustment == 0)
                throw new ArgumentException("Adjustment must be a non-zero value.", nameof(adjustment));

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _inventoryService.AdjustStockAsync(businessId, variantId, adjustment, reason);
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
