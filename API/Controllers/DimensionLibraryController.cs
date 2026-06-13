using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/dimension-library")]
    public class DimensionLibraryController : ControllerBase
    {
        private readonly IGetBusinessDimensionValuesUseCase _getValues;
        private readonly IGetCatalogDimensionCatalogUseCase _getCatalog;

        public DimensionLibraryController(
            IGetBusinessDimensionValuesUseCase getValues,
            IGetCatalogDimensionCatalogUseCase getCatalog)
        {
            _getValues = getValues;
            _getCatalog = getCatalog;
        }

        [HttpGet("catalog")]
        public IActionResult GetCatalog()
        {
            return Ok(_getCatalog.Execute());
        }

        [HttpGet("{businessId:guid}")]
        public async Task<IActionResult> GetByDimension(
            [FromRoute] Guid businessId,
            [FromQuery] string dimension,
            [FromQuery] bool includeInactive = false)
        {
            if (string.IsNullOrWhiteSpace(dimension))
            {
                return BadRequest("Query parameter 'dimension' is required.");
            }

            var values = await _getValues.ExecuteAsync(businessId, dimension, includeInactive);
            return Ok(values);
        }
    }
}
