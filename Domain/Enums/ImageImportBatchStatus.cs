namespace MiNegocioCR.Api.Domain.Enums;

public enum ImageImportBatchStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4,
}
