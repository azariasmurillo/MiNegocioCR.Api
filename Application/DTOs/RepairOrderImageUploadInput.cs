namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>Stream debe disponerse desde el llamador tras ExecuteAsync.</summary>
public sealed class RepairOrderImageUploadInput
{
    public required Stream Stream { get; init; }

    public required string ContentType { get; init; }

    public long Length { get; init; }
}
