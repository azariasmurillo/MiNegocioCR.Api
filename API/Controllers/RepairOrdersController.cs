using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RepairOrdersController : ControllerBase
{
    private const long MaxRepairOrderImageBytes = 5L * 1024 * 1024;
    private const int MaxRepairOrderImagesPerRequest = 5;
    private const long MaxRepairOrderImagesRequestBytes = 26L * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg"
    };

    private readonly ICreateRepairOrderUseCase _createRepairOrderUseCase;
    private readonly IUpdateRepairOrderStatusUseCase _updateStatusUseCase;
    private readonly IGetRepairOrdersByBusinessUseCase _getByBusinessUseCase;
    private readonly IGetRepairOrderByIdUseCase _getByIdUseCase;
    private readonly IUpdateRepairOrderUseCase _updateRepairOrderUseCase;
    private readonly IGetRepairOrderByBusinessIdAndStatusUseCase _getByIdAndStatusUseCase;
    private readonly ISearchRepairOrdersUseCase _searchRepairOrdersUseCase;
    private readonly ISendRepairOrderEmailUseCase _sendRepairOrderEmailUseCase;
    private readonly IChargeRepairOrderUseCase _chargeRepairOrderUseCase;
    private readonly IGetRepairOrderBalanceUseCase _getRepairOrderBalanceUseCase;
    private readonly IGetPaymentsByRepairOrderUseCase _getPaymentsByRepairOrderUseCase;
    private readonly IUploadRepairOrderImagesUseCase _uploadRepairOrderImagesUseCase;
    private readonly IGetRepairOrderImagesUseCase _getRepairOrderImagesUseCase;
    private readonly IDeleteRepairOrderImageUseCase _deleteRepairOrderImageUseCase;

    public RepairOrdersController(
    ICreateRepairOrderUseCase createRepairOrderUseCase,
    IUpdateRepairOrderStatusUseCase updateStatusUseCase,
    IGetRepairOrdersByBusinessUseCase getByBusinessUseCase,
    IGetRepairOrderByIdUseCase getByIdUseCase,
    IUpdateRepairOrderUseCase updateRepairOrderUseCase,
    IGetRepairOrderByBusinessIdAndStatusUseCase getByIdAndStatusUseCase,
    ISearchRepairOrdersUseCase searchRepairOrdersUseCase,
    ISendRepairOrderEmailUseCase sendRepairOrderEmailUseCase,
    IChargeRepairOrderUseCase chargeRepairOrderUseCase,
    IGetRepairOrderBalanceUseCase getRepairOrderBalanceUseCase,
    IGetPaymentsByRepairOrderUseCase getPaymentsByRepairOrderUseCase,
    IUploadRepairOrderImagesUseCase uploadRepairOrderImagesUseCase,
    IGetRepairOrderImagesUseCase getRepairOrderImagesUseCase,
    IDeleteRepairOrderImageUseCase deleteRepairOrderImageUseCase)
    {
        _createRepairOrderUseCase = createRepairOrderUseCase;
        _updateStatusUseCase = updateStatusUseCase;
        _getByBusinessUseCase = getByBusinessUseCase;
        _getByIdUseCase = getByIdUseCase;
        _updateRepairOrderUseCase = updateRepairOrderUseCase;
        _getByIdAndStatusUseCase = getByIdAndStatusUseCase;
        _searchRepairOrdersUseCase = searchRepairOrdersUseCase;
        _sendRepairOrderEmailUseCase = sendRepairOrderEmailUseCase;
        _chargeRepairOrderUseCase = chargeRepairOrderUseCase;
        _getRepairOrderBalanceUseCase = getRepairOrderBalanceUseCase;
        _getPaymentsByRepairOrderUseCase = getPaymentsByRepairOrderUseCase;
        _uploadRepairOrderImagesUseCase = uploadRepairOrderImagesUseCase;
        _getRepairOrderImagesUseCase = getRepairOrderImagesUseCase;
        _deleteRepairOrderImageUseCase = deleteRepairOrderImageUseCase;
    }

    /// <summary>POST multipart field <c>file</c> (hasta 5 archivos, PNG/JPEG, máx. 5 MB cada uno). Query <c>businessId</c>.</summary>
    [HttpPost("{repairOrderId:guid}/images")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRepairOrderImagesRequestBytes)]
    [RequestSizeLimit(MaxRepairOrderImagesRequestBytes)]
    public async Task<IActionResult> UploadRepairOrderImages(
        Guid repairOrderId,
        [FromQuery] Guid businessId,
        [FromForm(Name = "file")] List<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        if (businessId == Guid.Empty)
            return BadRequest("BusinessId is required.");
        if (files == null || files.Count == 0)
            return BadRequest("At least one file is required (form field: file).");
        if (files.Count > MaxRepairOrderImagesPerRequest)
            return BadRequest($"Maximum {MaxRepairOrderImagesPerRequest} images per request.");

        foreach (var file in files)
        {
            var normalizedType = NormalizeContentType(file.ContentType);
            if (normalizedType == null || !AllowedImageContentTypes.Contains(normalizedType))
                return BadRequest("Only image/png and image/jpeg are allowed.");
            if (file.Length == 0)
                return BadRequest("Empty file is not allowed.");
            if (file.Length > 0 && file.Length > MaxRepairOrderImageBytes)
                return BadRequest("Each image must be at most 5 MB.");
        }

        var inputs = new List<RepairOrderImageUploadInput>(files.Count);
        try
        {
            foreach (var file in files)
            {
                var normalizedType = NormalizeContentType(file.ContentType)!;
                await using var readStream = file.OpenReadStream();
                var ms = new MemoryStream();
                var buffer = new byte[8192];
                long total = 0;
                int bytesRead;
                while ((bytesRead = await readStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                {
                    total += bytesRead;
                    if (total > MaxRepairOrderImageBytes)
                        return BadRequest("Each image must be at most 5 MB.");
                    await ms.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }

                if (ms.Length == 0)
                    return BadRequest("Empty file is not allowed.");

                ms.Position = 0;
                inputs.Add(new RepairOrderImageUploadInput
                {
                    Stream = ms,
                    ContentType = normalizedType,
                    Length = ms.Length
                });
            }

            var result = await _uploadRepairOrderImagesUseCase.ExecuteAsync(
                businessId, repairOrderId, inputs, cancellationToken);
            return Ok(result);
        }
        finally
        {
            foreach (var input in inputs)
                await input.Stream.DisposeAsync();
        }
    }

    /// <summary>GET /api/RepairOrders/{repairOrderId}/images?businessId=…</summary>
    [HttpGet("{repairOrderId:guid}/images")]
    public async Task<IActionResult> GetRepairOrderImages(
        Guid repairOrderId,
        [FromQuery] Guid businessId,
        CancellationToken cancellationToken)
    {
        if (businessId == Guid.Empty)
            return BadRequest("BusinessId is required.");
        var result = await _getRepairOrderImagesUseCase.ExecuteAsync(businessId, repairOrderId, cancellationToken);
        return Ok(result);
    }

    /// <summary>DELETE /api/RepairOrders/images/{imageId}?businessId=…</summary>
    [HttpDelete("images/{imageId:guid}")]
    public async Task<IActionResult> DeleteRepairOrderImage(
        Guid imageId,
        [FromQuery] Guid businessId,
        CancellationToken cancellationToken)
    {
        if (businessId == Guid.Empty)
            return BadRequest("BusinessId is required.");
        await _deleteRepairOrderImageUseCase.ExecuteAsync(businessId, imageId, cancellationToken);
        return NoContent();
    }

    private static string? NormalizeContentType(string? raw)
    {
        return raw?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
    }

    [HttpPost("{businessId}")]
    public async Task<IActionResult> Create(
        Guid businessId,
        [FromBody] CreateRepairOrderRequestDto request)
    {
        if (request == null) return BadRequest("RepairOrdersCreate - Request body is required.");

        var result = await _createRepairOrderUseCase.Execute(businessId, request);
        return Ok(result);
    }

    [HttpPatch("{businessId:guid}/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
    Guid businessId,
    Guid id,
    [FromBody] UpdateStatusRequestDto request)
    {
        if (request == null) return BadRequest("RepairOrdersUpdateStatus - Request body is required.");
        var result = await _updateStatusUseCase.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/search")]
    public async Task<IActionResult> Search(
        Guid businessId,
        [FromQuery] string? query = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _searchRepairOrdersUseCase.Execute(businessId, query, from, to);
        return Ok(result);
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetByBusiness(Guid businessId)
    {
        var result = await _getByBusinessUseCase.Execute(businessId);
        return Ok(result);
    }

    /// <summary>GET /api/RepairOrders/{businessId}/{id}/payments — listado de pagos parciales de la orden.</summary>
    [HttpGet("{businessId:guid}/{id:guid}/payments")]
    public async Task<IActionResult> GetPayments(Guid businessId, Guid id)
    {
        var result = await _getPaymentsByRepairOrderUseCase.Execute(businessId, id);
        return Ok(result);
    }

    /// <summary>GET /api/RepairOrders/{businessId}/{id}/balance — mismo dato que …/{id}/balance?businessId=…</summary>
    [HttpGet("{businessId:guid}/{id:guid}/balance")]
    public async Task<IActionResult> GetBalanceForBusiness(Guid businessId, Guid id)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var result = await _getRepairOrderBalanceUseCase.Execute(businessId, id);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetById(Guid businessId, Guid id)
    {
        var result = await _getByIdUseCase.Execute(businessId, id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> Update(
    Guid businessId,
    Guid id,
    [FromBody] UpdateRepairOrderRequestDto request)
    {
        if (request == null) return BadRequest("RepairOrdersUpdate - Request body is required.");
        var result = await _updateRepairOrderUseCase.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpGet("{businessId}/by-status")]  // o "{businessId}/status"
    public async Task<IActionResult> GetByBusinessIdAndStatus(
    Guid businessId,
    [FromQuery] RepairOrderStatus status)
    {
        var result = await _getByIdAndStatusUseCase.Execute(businessId, status);
        return Ok(result);  
    }

    [HttpPost("{id:guid}/send-email")]
    public async Task<IActionResult> SendEmail(Guid id, [FromBody] SendEmailRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.HtmlContent))
            return BadRequest("htmlContent is required.");
        await _sendRepairOrderEmailUseCase.Execute(id, request.HtmlContent, request.Email);
        return Ok(new { message = "Email enviado correctamente" });
    }

    [HttpPost("{id:guid}/charge")]
    public async Task<IActionResult> Charge(Guid id, [FromQuery] Guid businessId, [FromBody] ChargeRepairOrderRequestDto? body = null)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var result = await _chargeRepairOrderUseCase.Execute(businessId, id, body?.PaymentMethods);
        return Ok(result);
    }

    [HttpGet("{id:guid}/balance")]
    public async Task<IActionResult> GetBalance(Guid id, [FromQuery] Guid businessId)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var result = await _getRepairOrderBalanceUseCase.Execute(businessId, id);
        return Ok(result);
    }

}



