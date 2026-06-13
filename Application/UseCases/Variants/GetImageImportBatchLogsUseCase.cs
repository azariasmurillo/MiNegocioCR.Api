using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class GetImageImportBatchLogsUseCase : IGetImageImportBatchLogsUseCase
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    private readonly IAppDbContext _context;

    public GetImageImportBatchLogsUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ImageImportLogDto>> ExecuteAsync(
        Guid businessId,
        Guid batchId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (batchId == Guid.Empty)
            throw new ArgumentException("BatchId is required.", nameof(batchId));

        if (page < 1)
            page = 1;

        if (pageSize <= 0)
            pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        var batchExists = await _context.ImageImportBatches
            .AnyAsync(b => b.Id == batchId && b.BusinessId == businessId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException("ImageImportBatch", "Lote de importación no encontrado.");

        var skip = (page - 1) * pageSize;

        return await _context.ImageImportLogs
            .AsNoTracking()
            .Where(l => l.BatchId == batchId)
            .OrderBy(l => l.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(l => new ImageImportLogDto
            {
                FileName = l.FileName,
                ParsedSku = l.ParsedSku,
                SortOrder = l.SortOrder,
                Status = l.Status.ToString(),
                Message = l.Message,
                CatalogVariantId = l.CatalogVariantId,
            })
            .ToListAsync(cancellationToken);
    }
}
