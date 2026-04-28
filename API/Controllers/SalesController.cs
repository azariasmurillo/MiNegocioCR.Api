using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly IRegisterSaleUseCase _registerSale;
        private readonly ICreateSaleFromRepairUseCase _createSaleFromRepair;

        public SalesController(
            IRegisterSaleUseCase registerSale,
            ICreateSaleFromRepairUseCase createSaleFromRepair)
        {
            _registerSale = registerSale;
            _createSaleFromRepair = createSaleFromRepair;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale(CreateSaleRequestDto request)
        {
            if (request == null) return BadRequest("CreateSale - Request body is required.");

            var businessId = request.BusinessId;
            if (businessId == Guid.Empty)
                return BadRequest("BusinessId is required.");

            if (request.Items == null || !request.Items.Any())
                return BadRequest("At least one item is required.");

            request.Source = "Manual";
            var result = await _registerSale.ExecuteAsync(request);
            return Ok(result);
        }

        [HttpPost("from-repair/{repairOrderId:guid}")]
        public async Task<IActionResult> CreateFromRepair(
            Guid repairOrderId,
            [FromBody] CreateSaleFromRepairRequestDto request)
        {
            if (request == null) return BadRequest("CreateFromRepair - Request body is required.");
            if (request.BusinessId == Guid.Empty) return BadRequest("BusinessId is required.");
            var businessId = request.BusinessId;
            var result = await _createSaleFromRepair.ExecuteAsync(businessId, repairOrderId, request);
            return Ok(result);
        }

        [HttpPost("from-whatsapp")]
        public async Task<IActionResult> CreateFromWhatsapp(CreateSaleRequestDto request)
        {
            if (request == null) return BadRequest("CreateFromWhatsapp - Request body is required.");
            if (request.BusinessId == Guid.Empty) return BadRequest("BusinessId is required.");
            if (request.Items == null || !request.Items.Any()) return BadRequest("At least one item is required.");

            request.Source = "WhatsApp";
            var result = await _registerSale.ExecuteAsync(request);
            return Ok(result);
        }
    }
}
