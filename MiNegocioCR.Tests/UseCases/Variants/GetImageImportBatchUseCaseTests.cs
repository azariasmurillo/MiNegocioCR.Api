using MiNegocioCR.Api.Application.UseCases.Variants;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Tests.UseCases.Variants;

public class GetImageImportBatchUseCaseTests
{
    [Fact]
    public void ResolveDisplayStatus_maps_stuck_processing_to_completed_when_all_files_processed()
    {
        var batch = new ImageImportBatch
        {
            Status = ImageImportBatchStatus.Processing,
            TotalFiles = 3,
            ProcessedFiles = 3,
            SuccessCount = 3,
        };

        Assert.Equal("Completed", GetImageImportBatchUseCase.ResolveDisplayStatus(batch));
    }

    [Fact]
    public void ResolveDisplayStatus_keeps_processing_while_files_remain()
    {
        var batch = new ImageImportBatch
        {
            Status = ImageImportBatchStatus.Processing,
            TotalFiles = 3,
            ProcessedFiles = 1,
        };

        Assert.Equal("Processing", GetImageImportBatchUseCase.ResolveDisplayStatus(batch));
    }
}
