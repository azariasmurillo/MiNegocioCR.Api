using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly CreateCatalogItemUseCase _createCatalogItem;

        public CatalogController(CreateCatalogItemUseCase createCatalogItem)
        {
            _createCatalogItem = createCatalogItem;
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(CreateCatalogItemRequestDto request)
        {
            if (request == null) return BadRequest("Request body is required.");
            var id = await _createCatalogItem.ExecuteAsync(
                request.BusinessId,
                request.Name,
                request.BasePrice,
                request.TrackStock,
                request.Type
            );

            return Ok(id);
        }
    }
}
