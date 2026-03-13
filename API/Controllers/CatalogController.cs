using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ICreateCatalogItemUseCase _createCatalogItem;

        public CatalogController(ICreateCatalogItemUseCase createCatalogItem)
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
