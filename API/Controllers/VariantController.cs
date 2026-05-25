using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Variants;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/variants")]
    public class VariantController : ControllerBase
    {
        private const long MaxVariantImageBytes = 5L * 1024 * 1024;
        private const int MaxVariantImagesPerRequest = 3;
        private const long MaxVariantImagesRequestBytes = 16L * 1024 * 1024;

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png",
            "image/jpeg"
        };

        private readonly ICreateVariantUseCase _createVariant;
        private readonly IUpdateVariantUseCase _updateVariant;
        private readonly IDeleteVariantUseCase _deleteVariant;
        private readonly IGetVariantsByCatalogItemUseCase _getVariantsByCatalogItem;
        private readonly IGetVariantsByBusinessUseCase _getVariantsByBusiness;
        private readonly IUploadCatalogVariantImagesUseCase _uploadCatalogVariantImagesUseCase;
        private readonly IGetCatalogVariantImagesUseCase _getCatalogVariantImagesUseCase;
        private readonly IDeleteCatalogVariantImageUseCase _deleteCatalogVariantImageUseCase;
        private readonly ISetPrimaryCatalogVariantImageUseCase _setPrimaryCatalogVariantImageUseCase;

        public VariantController(
            ICreateVariantUseCase createVariant,
            IUpdateVariantUseCase updateVariant,
            IDeleteVariantUseCase deleteVariant,
            IGetVariantsByCatalogItemUseCase getVariantsByCatalogItem,
            IGetVariantsByBusinessUseCase getVariantsByBusiness,
            IUploadCatalogVariantImagesUseCase uploadCatalogVariantImagesUseCase,
            IGetCatalogVariantImagesUseCase getCatalogVariantImagesUseCase,
            IDeleteCatalogVariantImageUseCase deleteCatalogVariantImageUseCase,
            ISetPrimaryCatalogVariantImageUseCase setPrimaryCatalogVariantImageUseCase)
        {
            _createVariant = createVariant;
            _updateVariant = updateVariant;
            _deleteVariant = deleteVariant;
            _getVariantsByCatalogItem = getVariantsByCatalogItem;
            _getVariantsByBusiness = getVariantsByBusiness;
            _uploadCatalogVariantImagesUseCase = uploadCatalogVariantImagesUseCase;
            _getCatalogVariantImagesUseCase = getCatalogVariantImagesUseCase;
            _deleteCatalogVariantImageUseCase = deleteCatalogVariantImageUseCase;
            _setPrimaryCatalogVariantImageUseCase = setPrimaryCatalogVariantImageUseCase;
        }

        [HttpGet("{catalogItemId:guid}")]
        public async Task<IActionResult> GetVariantsByCatalogItem(
            [FromRoute] Guid catalogItemId,
            [FromQuery] Guid businessId)
        {
            if (businessId == Guid.Empty)
                return BadRequest("businessId query parameter is required.");

            var items = await _getVariantsByCatalogItem.ExecuteAsync(catalogItemId, businessId);
            return Ok(items);
        }

        [HttpGet("business/{businessId:guid}")]
        public async Task<IActionResult> GetVariantsByBusiness(
            [FromRoute] Guid businessId,
            [FromQuery] Guid? catalogItemId = null,
            [FromQuery] string? search = null)
        {
            var items = await _getVariantsByBusiness.ExecuteAsync(businessId, catalogItemId, search);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVariant(CreateVariantRequestDto request)
        {
            if (request == null) return BadRequest("CreateVariant - Request body is required.");

            var id = await _createVariant.ExecuteAsync(request);

            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVariant([FromRoute] Guid id, [FromBody] UpdateVariantRequestDto request)
        {
            if (request == null) return BadRequest("Request body is required.");

            await _updateVariant.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVariant([FromRoute] Guid id, [FromQuery] Guid businessId)
        {
            await _deleteVariant.ExecuteAsync(id, businessId);
            return NoContent();
        }

        /// <summary>Multipart field <c>file</c> (máx. 3 por variante en total). Query <c>businessId</c>.</summary>
        [HttpPost("{variantId:guid}/images")]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxVariantImagesRequestBytes)]
        [RequestSizeLimit(MaxVariantImagesRequestBytes)]
        public async Task<IActionResult> UploadVariantImages(
            Guid variantId,
            [FromQuery] Guid businessId,
            [FromForm(Name = "file")] List<IFormFile>? files,
            CancellationToken cancellationToken)
        {
            if (businessId == Guid.Empty)
                return BadRequest("BusinessId is required.");
            if (files == null || files.Count == 0)
                return BadRequest("At least one file is required (form field: file).");
            if (files.Count > MaxVariantImagesPerRequest)
                return BadRequest($"Maximum {MaxVariantImagesPerRequest} images per request.");

            foreach (var file in files)
            {
                var normalizedType = NormalizeContentType(file.ContentType);
                if (normalizedType == null || !AllowedImageContentTypes.Contains(normalizedType))
                    return BadRequest("Only image/png and image/jpeg are allowed.");
                if (file.Length == 0)
                    return BadRequest("Empty file is not allowed.");
                if (file.Length > 0 && file.Length > MaxVariantImageBytes)
                    return BadRequest("Each image must be at most 5 MB.");
            }

            var inputs = new List<CatalogVariantImageUploadInput>(files.Count);
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
                        if (total > MaxVariantImageBytes)
                            return BadRequest("Each image must be at most 5 MB.");
                        await ms.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    }

                    if (ms.Length == 0)
                        return BadRequest("Empty file is not allowed.");

                    ms.Position = 0;
                    inputs.Add(new CatalogVariantImageUploadInput
                    {
                        Stream = ms,
                        ContentType = normalizedType,
                        Length = ms.Length
                    });
                }

                var result = await _uploadCatalogVariantImagesUseCase.ExecuteAsync(
                    businessId, variantId, inputs, cancellationToken);
                return Ok(result);
            }
            finally
            {
                foreach (var input in inputs)
                    await input.Stream.DisposeAsync();
            }
        }

        [HttpGet("{variantId:guid}/images")]
        public async Task<IActionResult> GetVariantImages(
            Guid variantId,
            [FromQuery] Guid businessId,
            CancellationToken cancellationToken)
        {
            if (businessId == Guid.Empty)
                return BadRequest("BusinessId is required.");
            var result = await _getCatalogVariantImagesUseCase.ExecuteAsync(businessId, variantId, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("images/{imageId:guid}")]
        public async Task<IActionResult> DeleteVariantImage(
            Guid imageId,
            [FromQuery] Guid businessId,
            CancellationToken cancellationToken)
        {
            if (businessId == Guid.Empty)
                return BadRequest("BusinessId is required.");
            await _deleteCatalogVariantImageUseCase.ExecuteAsync(businessId, imageId, cancellationToken);
            return NoContent();
        }

        [HttpPatch("images/{imageId:guid}/primary")]
        public async Task<IActionResult> SetPrimaryVariantImage(
            Guid imageId,
            [FromQuery] Guid businessId,
            CancellationToken cancellationToken)
        {
            if (businessId == Guid.Empty)
                return BadRequest("BusinessId is required.");
            var result = await _setPrimaryCatalogVariantImageUseCase.ExecuteAsync(businessId, imageId, cancellationToken);
            return Ok(result);
        }

        private static string? NormalizeContentType(string? raw)
        {
            return raw?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
        }
    }
}
