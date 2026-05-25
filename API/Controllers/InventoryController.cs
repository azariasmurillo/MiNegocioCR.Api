using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IAdjustInventoryUseCase _adjustInventory;

        public InventoryController(IAdjustInventoryUseCase adjustInventory)
        {
            _adjustInventory = adjustInventory;
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock(AdjustInventoryRequestDto request)
        {
            if (request == null) return BadRequest("AdjustStock - Request body is required.");

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
