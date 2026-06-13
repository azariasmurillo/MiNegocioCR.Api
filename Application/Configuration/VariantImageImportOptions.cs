namespace MiNegocioCR.Api.Application.Configuration;

public class VariantImageImportOptions
{
    public const string SectionName = "VariantImageImport";

    public long MaxZipBytes { get; set; } = 104_857_600;

    public long MaxImageBytes { get; set; } = 10_485_760;

    public int MaxFilesPerZip { get; set; } = 500;

    public int MaxImagesPerVariant { get; set; } = 3;

    public int WebpQuality { get; set; } = 88;

    public string StagingDirectory { get; set; } = string.Empty;
}
