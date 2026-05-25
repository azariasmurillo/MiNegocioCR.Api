using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
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

            var lines = (request.Items ?? new List<RegisterPurchaseLineDto>())
                .Select(x => (x.VariantId, x.Quantity, x.Cost))
                .ToList();

            await _registerPurchase.ExecuteAsync(request.BusinessId, lines);

            return Ok();
        }
    }
}
