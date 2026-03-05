using MiNegocioCR.Api.Application.Interfaces.Services;

namespace MiNegocioCR.Api.Application.UseCases.Inventory
{

    public class RegisterSaleUseCase
    {
        private readonly IInventoryService _inventoryService;

        public RegisterSaleUseCase(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task ExecuteAsync(
            Guid businessId,
            Guid variantId,
            int quantity)
        {
            await _inventoryService.DecreaseStockAsync(
                businessId,
                variantId,
                quantity,
                "Sale"
            );
        }
    }    
}
