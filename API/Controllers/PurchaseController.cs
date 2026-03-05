using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.UseCases.Repository;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchaseController : ControllerBase
    {
        private readonly RegisterPurchaseUseCase _registerPurchase;

        public PurchaseController(RegisterPurchaseUseCase registerPurchase)
        {
            _registerPurchase = registerPurchase;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPurchase(RegisterPurchaseRequestDto request)
        {
            await _registerPurchase.ExecuteAsync(
                request.BusinessId,
                request.VariantId,
                request.Quantity,
                request.Cost
            );

            return Ok();
        }
    }
}
