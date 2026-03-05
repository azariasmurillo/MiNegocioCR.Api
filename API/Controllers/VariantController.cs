using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.UseCases.Repository;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/variants")]
    public class VariantController : ControllerBase
    {
        private readonly CreateVariantUseCase _createVariant;

        public VariantController(CreateVariantUseCase createVariant)
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
