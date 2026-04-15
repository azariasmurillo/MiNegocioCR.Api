using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/option-values")]
    public class OptionValuesController : ControllerBase
    {
        private readonly ICreateOptionValueUseCase _createOptionValue;
        private readonly IGetValuesByOptionUseCase _getValuesByOption;
        private readonly IUpdateOptionValueUseCase _updateOptionValue;
        private readonly IToggleOptionValueStatusUseCase _toggleOptionValueStatus;
        private readonly IDeleteOptionValueUseCase _deleteOptionValue;

        public OptionValuesController(
            ICreateOptionValueUseCase createOptionValue,
            IGetValuesByOptionUseCase getValuesByOption,
            IUpdateOptionValueUseCase updateOptionValue,
            IToggleOptionValueStatusUseCase toggleOptionValueStatus,
            IDeleteOptionValueUseCase deleteOptionValue)
        {
            _createOptionValue = createOptionValue;
            _getValuesByOption = getValuesByOption;
            _updateOptionValue = updateOptionValue;
            _toggleOptionValueStatus = toggleOptionValueStatus;
            _deleteOptionValue = deleteOptionValue;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCatalogOptionValueRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var id = await _createOptionValue.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateOptionValueRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _updateOptionValue.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpPatch("{id:guid}/toggle")]
        public async Task<IActionResult> Toggle([FromRoute] Guid id, [FromBody] ToggleOptionValueStatusRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _toggleOptionValueStatus.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _deleteOptionValue.ExecuteAsync(id);
            return NoContent();
        }

        [HttpGet("{optionId:guid}")]
        public async Task<IActionResult> GetByOption([FromRoute] Guid optionId, [FromQuery] bool includeInactive = false)
        {
            var values = await _getValuesByOption.ExecuteAsync(optionId, includeInactive);
            return Ok(values);
        }
    }
}
