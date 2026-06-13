using System.Text.RegularExpressions;

namespace MiNegocioCR.Api.Application.Common;

public sealed partial class SkuImageFileNameParser
{
    [GeneratedRegex(
        @"^(?<sku>(?:[A-Za-z0-9][A-Za-z0-9._-]{0,78}[A-Za-z0-9]|[A-Za-z0-9]))_(?<slot>[1-3])\.(?<ext>jpe?g|png|webp)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FilePattern();

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "webp",
    };

    public static bool TryParse(string? rawPath, out SkuImageFileParseResult result)
    {
        result = default!;
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            return false;
        }

        var fileName = Path.GetFileName(rawPath.Trim());
        var match = FilePattern().Match(fileName);
        if (!match.Success)
        {
            return false;
        }

        var ext = match.Groups["ext"].Value.ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return false;
        }

        if (!int.TryParse(match.Groups["slot"].Value, out var slot) || slot is < 1 or > 3)
        {
            return false;
        }

        result = new SkuImageFileParseResult(
            fileName,
            match.Groups["sku"].Value,
            slot,
            ext);
        return true;
    }
}

public readonly record struct SkuImageFileParseResult(
    string FileName,
    string Sku,
    int SortOrder,
    string Extension);
