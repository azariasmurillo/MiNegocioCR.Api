using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class GetImageImportBatchUseCase : IGetImageImportBatchUseCase
{
    private readonly IAppDbContext _context;

    public GetImageImportBatchUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ImageImportBatchDto> ExecuteAsync(
        Guid businessId,
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (batchId == Guid.Empty)
            throw new ArgumentException("BatchId is required.", nameof(batchId));

        var batch = await _context.ImageImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && b.BusinessId == businessId, cancellationToken);

        if (batch == null)
            throw new NotFoundException("ImageImportBatch", "Lote de importación no encontrado.");

        var status = ResolveDisplayStatus(batch);

        return new ImageImportBatchDto
        {
            Id = batch.Id,
            Status = status,
            TotalFiles = batch.TotalFiles,
            ProcessedFiles = batch.ProcessedFiles,
            SuccessCount = batch.SuccessCount,
            SkippedCount = batch.SkippedCount,
            ErrorCount = batch.ErrorCount,
            CreatedAt = batch.CreatedAt,
            CompletedAt = batch.CompletedAt,
            SummaryMessage = batch.SummaryMessage,
        };
    }

    internal static string ResolveDisplayStatus(Domain.Entities.ImageImportBatch batch)
    {
        if (batch.Status == ImageImportBatchStatus.Processing
            && batch.TotalFiles > 0
            && batch.ProcessedFiles >= batch.TotalFiles)
        {
            return batch.ErrorCount > 0
                ? ImageImportBatchStatus.CompletedWithErrors.ToString()
                : ImageImportBatchStatus.Completed.ToString();
        }

        return batch.Status.ToString();
    }
}
