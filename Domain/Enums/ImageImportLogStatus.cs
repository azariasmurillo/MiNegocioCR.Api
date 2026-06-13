namespace MiNegocioCR.Api.Domain.Enums;

public enum ImageImportLogStatus
{
    Success = 0,
    SkippedExisting = 1,
    VariantNotFound = 2,
    AmbiguousSku = 3,
    InvalidFileName = 4,
    InvalidSlot = 5,
    UnsupportedFormat = 6,
    DuplicateSlotInZip = 7,
    MaxImagesExceeded = 8,
    ProcessingFailed = 9,
    StorageFailed = 10,
}
