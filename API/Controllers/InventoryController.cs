using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly AdjustInventoryUseCase _adjustInventory;

        public InventoryController(AdjustInventoryUseCase adjustInventory)
        {
            _adjustInventory = adjustInventory;
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock(AdjustInventoryRequestDto request)
        {
            await _adjustInventory.ExecuteAsync(
                request.BusinessId,
                request.VariantId,
                request.Adjustment,
                request.Reason
            );

            return Ok();
        }
    }
}
