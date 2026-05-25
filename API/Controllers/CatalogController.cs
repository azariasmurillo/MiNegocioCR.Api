using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiNegocioCR.Api.API.Helpers;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ICreateCatalogItemUseCase _createCatalogItem;
        private readonly IUpdateCatalogItemUseCase _updateCatalogItem;
        private readonly IToggleCatalogItemStatusUseCase _toggleCatalogItemStatus;
        private readonly IDeleteCatalogItemUseCase _deleteCatalogItem;
        private readonly IGetCatalogItemsByBusinessUseCase _getCatalogItemsByBusiness;

        public CatalogController(
            ICreateCatalogItemUseCase createCatalogItem,
            IUpdateCatalogItemUseCase updateCatalogItem,
            IToggleCatalogItemStatusUseCase toggleCatalogItemStatus,
            IDeleteCatalogItemUseCase deleteCatalogItem,
            IGetCatalogItemsByBusinessUseCase getCatalogItemsByBusiness)
        {
            _createCatalogItem = createCatalogItem;
            _updateCatalogItem = updateCatalogItem;
            _toggleCatalogItemStatus = toggleCatalogItemStatus;
            _deleteCatalogItem = deleteCatalogItem;
            _getCatalogItemsByBusiness = getCatalogItemsByBusiness;
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateCatalogItemRequestDto request)
        {
            if (request == null) return BadRequest("Request body is required.");

            var id = await _createCatalogItem.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateItem([FromRoute] Guid id, [FromBody] UpdateCatalogItemRequestDto request)
        {
            if (request == null) return BadRequest("Request body is required.");

            await _updateCatalogItem.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpPatch("{id:guid}/toggle")]
        public async Task<IActionResult> ToggleItemStatus([FromRoute] Guid id, [FromBody] ToggleCatalogItemStatusRequestDto request)
        {
            if (request == null) return BadRequest("Request body is required.");

            await _toggleCatalogItemStatus.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem([FromRoute] Guid id, [FromQuery] Guid businessId)
        {
            await _deleteCatalogItem.ExecuteAsync(id, businessId);
            return NoContent();
        }

        [HttpGet("{businessId:guid}")]
        public async Task<IActionResult> GetItemsByBusiness([FromRoute] Guid businessId, [FromQuery] bool includeInactive = false)
        {
            var items = await _getCatalogItemsByBusiness.ExecuteAsync(businessId, includeInactive);
            return Ok(items);
        }

        // Multi-tenant seguro: BusinessId SIEMPRE desde JWT, nunca desde el cliente.
        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyBusinessItems([FromQuery] bool includeInactive = false)
        {
            var businessId = AuthHelper.GetBusinessId(HttpContext);
            if (!businessId.HasValue || businessId.Value == Guid.Empty)
            {
                return Unauthorized(new { sessionMessage = "Token inválido para negocio." });
            }

            var items = await _getCatalogItemsByBusiness.ExecuteAsync(businessId.Value, includeInactive);
            return Ok(items);
        }
    }
}
