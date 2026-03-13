using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchaseController : ControllerBase
    {
        private readonly IRegisterPurchaseUseCase _registerPurchase;

        public PurchaseController(IRegisterPurchaseUseCase registerPurchase)
        {
            _registerPurchase = registerPurchase;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPurchase(RegisterPurchaseRequestDto request)
        {
            if (request == null) return BadRequest("RegisterPurchase - Request body is required.");

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
