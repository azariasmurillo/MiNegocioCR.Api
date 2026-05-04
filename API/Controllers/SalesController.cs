using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Http;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly IRegisterSaleUseCase _registerSale;
        private readonly ICreateSaleFromRepairUseCase _createSaleFromRepair;
        private readonly ISendSaleEmailUseCase _sendSaleEmailUseCase;
        private readonly IGetSalesByBusinessUseCase _getSalesByBusinessUseCase;
        private readonly IGetSaleByIdUseCase _getSaleByIdUseCase;

        public SalesController(
            IRegisterSaleUseCase registerSale,
            ICreateSaleFromRepairUseCase createSaleFromRepair,
            ISendSaleEmailUseCase sendSaleEmailUseCase,
            IGetSalesByBusinessUseCase getSalesByBusinessUseCase,
            IGetSaleByIdUseCase getSaleByIdUseCase)
        {
            _registerSale = registerSale;
            _createSaleFromRepair = createSaleFromRepair;
            _sendSaleEmailUseCase = sendSaleEmailUseCase;
            _getSalesByBusinessUseCase = getSalesByBusinessUseCase;
            _getSaleByIdUseCase = getSaleByIdUseCase;
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

        [HttpPost("{id:guid}/send-email")]
        public async Task<IActionResult> SendEmail(Guid id, [FromBody] SendEmailRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.HtmlContent))
                return BadRequest("htmlContent is required.");
            await _sendSaleEmailUseCase.Execute(id, request.HtmlContent, request.Email);
            return Ok(new { message = "Email enviado correctamente" });
        }

        /// <summary>GET /api/sales/{id}?businessId=… — si viene businessId y no coincide con la venta, 404.</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid? businessId = null)
        {
            var result = await _getSaleByIdUseCase.ExecuteAsync(id, businessId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>GET /api/sales/business/{businessId}/{id}</summary>
        [HttpGet("business/{businessId:guid}/{id:guid}")]
        public async Task<IActionResult> GetByBusinessAndId(Guid businessId, Guid id)
        {
            var result = await _getSaleByIdUseCase.ExecuteAsync(id, businessId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("business/{businessId:guid}")]
        public async Task<IActionResult> GetByBusiness(
            Guid businessId,
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] string? search,
            [FromQuery] string? page = null,
            [FromQuery] string? pageSize = null,
            [FromQuery] string? sort = "createdAt desc",
            [FromQuery] string? paymentMethod = null)
        {
            if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");

            var query = new SalesListQueryDto
            {
                From = QueryParamParsing.ParseUtcDayStart(from),
                To = QueryParamParsing.ParseUtcDayStart(to),
                Search = search,
                Page = QueryParamParsing.ParsePositiveInt(page, 1, 1_000_000),
                PageSize = QueryParamParsing.ParsePositiveInt(pageSize, 20, 100),
                Sort = sort,
                PaymentMethod = paymentMethod
            };

            var result = await _getSalesByBusinessUseCase.Execute(businessId, query);
            return Ok(result);
        }
    }
}
