using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly IRegisterSaleUseCase _registerSale;

        public SalesController(IRegisterSaleUseCase registerSale)
        {
            _registerSale = registerSale;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale(CreateSaleRequestDto request)
        {
            if (request == null) return BadRequest("CreateSale - Request body is required.");

            var businessIdClaim = User.FindFirst("businessId")?.Value;
            if (string.IsNullOrEmpty(businessIdClaim) || !Guid.TryParse(businessIdClaim, out var businessId))
                return BadRequest("Invalid or missing businessId claim.");

            if (request.Items == null || !request.Items.Any())
                return BadRequest("At least one item is required.");

            var id = await _registerSale.ExecuteAsync(
                businessId,
                request.Items
                    .Select(x => (x.VariantId, x.Quantity, x.Price))
                    .ToList());

            return Ok(id);
        }
    }
}
