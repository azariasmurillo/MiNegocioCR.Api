using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/variants")]
    public class VariantController : ControllerBase
    {
        private readonly ICreateVariantUseCase _createVariant;

        public VariantController(ICreateVariantUseCase createVariant)
        {
            _createVariant = createVariant;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVariant(CreateVariantRequestDto request)
        {
            if (request == null) return BadRequest("CreateVariant - Request body is required.");

            var id = await _createVariant.ExecuteAsync(
                request.CatalogItemId,
                request.SKU,
                request.Price,
                request.InitialStock
            );

            return Ok(id);
        }
    }
}
