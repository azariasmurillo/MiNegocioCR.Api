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
        private readonly IUpdateVariantUseCase _updateVariant;
        private readonly IDeleteVariantUseCase _deleteVariant;
        private readonly IGetVariantsByCatalogItemUseCase _getVariantsByCatalogItem;
        private readonly IGetVariantsByBusinessUseCase _getVariantsByBusiness;

        public VariantController(
            ICreateVariantUseCase createVariant,
            IUpdateVariantUseCase updateVariant,
            IDeleteVariantUseCase deleteVariant,
            IGetVariantsByCatalogItemUseCase getVariantsByCatalogItem,
            IGetVariantsByBusinessUseCase getVariantsByBusiness)
        {
            _createVariant = createVariant;
            _updateVariant = updateVariant;
            _deleteVariant = deleteVariant;
            _getVariantsByCatalogItem = getVariantsByCatalogItem;
            _getVariantsByBusiness = getVariantsByBusiness;
        }

        [HttpGet("{catalogItemId:guid}")]
        public async Task<IActionResult> GetVariantsByCatalogItem(
            [FromRoute] Guid catalogItemId,
            [FromQuery] Guid businessId)
        {
            if (businessId == Guid.Empty)
                return BadRequest("businessId query parameter is required.");

            var items = await _getVariantsByCatalogItem.ExecuteAsync(catalogItemId, businessId);
            return Ok(items);
        }

        [HttpGet("business/{businessId:guid}")]
        public async Task<IActionResult> GetVariantsByBusiness(
            [FromRoute] Guid businessId,
            [FromQuery] Guid? catalogItemId = null,
            [FromQuery] string? search = null)
        {
            var items = await _getVariantsByBusiness.ExecuteAsync(businessId, catalogItemId, search);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVariant(CreateVariantRequestDto request)
        {
            if (request == null) return BadRequest("CreateVariant - Request body is required.");

            var id = await _createVariant.ExecuteAsync(request);

            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVariant([FromRoute] Guid id, [FromBody] UpdateVariantRequestDto request)
        {
            if (request == null) return BadRequest("Request body is required.");

            await _updateVariant.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVariant([FromRoute] Guid id, [FromQuery] Guid businessId)
        {
            await _deleteVariant.ExecuteAsync(id, businessId);
            return NoContent();
        }
    }
}
