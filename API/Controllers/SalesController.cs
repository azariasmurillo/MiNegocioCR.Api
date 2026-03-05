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
            var businessId = Guid.Parse(
                User.FindFirst("businessId")!.Value
            );

            var id = await _registerSale.ExecuteAsync(
                businessId,
                request.Items
                    .Select(x => (x.VariantId, x.Quantity, x.Price))
                    .ToList());

            return Ok(id);
        }
    }
}
