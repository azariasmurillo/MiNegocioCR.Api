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

        public OptionValuesController(
            ICreateOptionValueUseCase createOptionValue,
            IGetValuesByOptionUseCase getValuesByOption)
        {
            _createOptionValue = createOptionValue;
            _getValuesByOption = getValuesByOption;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCatalogOptionValueRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var id = await _createOptionValue.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpGet("{optionId:guid}")]
        public async Task<IActionResult> GetByOption([FromRoute] Guid optionId)
        {
            var values = await _getValuesByOption.ExecuteAsync(optionId);
            return Ok(values);
        }
    }
}
