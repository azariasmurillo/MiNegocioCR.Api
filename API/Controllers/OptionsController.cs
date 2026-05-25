using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/options")]
    public class OptionsController : ControllerBase
    {
        private readonly ICreateOptionUseCase _createOption;
        private readonly IGetOptionsByItemUseCase _getOptionsByItem;
        private readonly IUpdateOptionUseCase _updateOption;
        private readonly IToggleOptionStatusUseCase _toggleOptionStatus;
        private readonly IDeleteOptionUseCase _deleteOption;

        public OptionsController(
            ICreateOptionUseCase createOption,
            IGetOptionsByItemUseCase getOptionsByItem,
            IUpdateOptionUseCase updateOption,
            IToggleOptionStatusUseCase toggleOptionStatus,
            IDeleteOptionUseCase deleteOption)
        {
            _createOption = createOption;
            _getOptionsByItem = getOptionsByItem;
            _updateOption = updateOption;
            _toggleOptionStatus = toggleOptionStatus;
            _deleteOption = deleteOption;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCatalogOptionRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var id = await _createOption.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateOptionRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _updateOption.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpPatch("{id:guid}/toggle")]
        public async Task<IActionResult> Toggle([FromRoute] Guid id, [FromBody] ToggleOptionStatusRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _toggleOptionStatus.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _deleteOption.ExecuteAsync(id);
            return NoContent();
        }

        [HttpGet("{catalogItemId:guid}")]
        public async Task<IActionResult> GetByCatalogItem([FromRoute] Guid catalogItemId, [FromQuery] bool includeInactive = false)
        {
            var options = await _getOptionsByItem.ExecuteAsync(catalogItemId, includeInactive);
            return Ok(options);
        }
    }
}
