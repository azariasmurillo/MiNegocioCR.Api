using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Helpers;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Configuration;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using Microsoft.Extensions.Options;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/catalog/variant-images")]
public class CatalogVariantImagesController : ControllerBase
{
    private readonly IStartVariantImageImportZipUseCase _startImport;
    private readonly IGetImageImportBatchUseCase _getBatch;
    private readonly IGetImageImportBatchLogsUseCase _getBatchLogs;
    private readonly VariantImageImportOptions _options;

    public CatalogVariantImagesController(
        IStartVariantImageImportZipUseCase startImport,
        IGetImageImportBatchUseCase getBatch,
        IGetImageImportBatchLogsUseCase getBatchLogs,
        IOptions<VariantImageImportOptions> options)
    {
        _startImport = startImport;
        _getBatch = getBatch;
        _getBatchLogs = getBatchLogs;
        _options = options.Value;
    }

    [HttpPost("import-zip")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> ImportZip(
        [FromForm] ImportZipFormRequest request,
        CancellationToken cancellationToken = default)
    {
        var businessId = AuthHelper.GetBusinessId(HttpContext);
        if (!businessId.HasValue || businessId.Value == Guid.Empty)
            return Unauthorized(new { sessionMessage = "Token inválido para negocio." });

        var userId = AuthHelper.GetUserId(HttpContext);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return Unauthorized(new { sessionMessage = "Token inválido." });

        var file = request.File;
        if (file == null || file.Length == 0)
            return BadRequest("Se requiere un archivo ZIP (campo file).");

        if (file.Length > _options.MaxZipBytes)
            return BadRequest($"El ZIP no puede superar {_options.MaxZipBytes / (1024 * 1024)} MB.");

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo se aceptan archivos .zip.");

        var style = string.IsNullOrWhiteSpace(request.MarketplaceStyle)
            ? MarketplaceStylePresets.WhiteV1
            : request.MarketplaceStyle.Trim();

        try
        {
            await using var stream = file.OpenReadStream();
            var batchId = await _startImport.ExecuteAsync(
                new StartVariantImageImportZipInput
                {
                    BusinessId = businessId.Value,
                    CreatedByUserId = userId.Value,
                    ZipStream = stream,
                    OriginalFileName = file.FileName,
                    ZipLength = file.Length,
                    ReplaceExisting = request.ReplaceExisting,
                    UseBackgroundRemoval = request.UseBackgroundRemoval || request.UseAiProcessing,
                    MarketplaceStyle = style,
                },
                cancellationToken);

            return Accepted(new { batchId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public sealed class ImportZipFormRequest
    {
        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }

        public bool ReplaceExisting { get; set; }

        public bool UseBackgroundRemoval { get; set; }

        public bool UseAiProcessing { get; set; }

        public string? MarketplaceStyle { get; set; }
    }

    [HttpGet("import-batches/{batchId:guid}")]
    public async Task<IActionResult> GetBatch(Guid batchId, CancellationToken cancellationToken)
    {
        var businessId = AuthHelper.GetBusinessId(HttpContext);
        if (!businessId.HasValue || businessId.Value == Guid.Empty)
            return Unauthorized(new { sessionMessage = "Token inválido para negocio." });

        var batch = await _getBatch.ExecuteAsync(businessId.Value, batchId, cancellationToken);
        return Ok(batch);
    }

    [HttpGet("import-batches/{batchId:guid}/logs")]
    public async Task<IActionResult> GetBatchLogs(
        Guid batchId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var businessId = AuthHelper.GetBusinessId(HttpContext);
        if (!businessId.HasValue || businessId.Value == Guid.Empty)
            return Unauthorized(new { sessionMessage = "Token inválido para negocio." });

        var logs = await _getBatchLogs.ExecuteAsync(
            businessId.Value,
            batchId,
            page,
            pageSize,
            cancellationToken);
        return Ok(logs);
    }
}
