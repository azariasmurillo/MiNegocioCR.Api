using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/options")]
    public class OptionsController : ControllerBase
    {
        private readonly ICreateOptionUseCase _createOption;
        private readonly IGetOptionsByItemUseCase _getOptionsByItem;

        public OptionsController(
            ICreateOptionUseCase createOption,
            IGetOptionsByItemUseCase getOptionsByItem)
        {
            _createOption = createOption;
            _getOptionsByItem = getOptionsByItem;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCatalogOptionRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var id = await _createOption.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpGet("{catalogItemId:guid}")]
        public async Task<IActionResult> GetByCatalogItem([FromRoute] Guid catalogItemId)
        {
            var options = await _getOptionsByItem.ExecuteAsync(catalogItemId);
            return Ok(options);
        }
    }
}
