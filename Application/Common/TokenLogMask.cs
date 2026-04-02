namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Avoids logging full secrets. Use <see cref="MaskLastFour"/> for bearer tokens; <see cref="TruncateForLog"/> for long API error bodies.
/// </summary>
public static class TokenLogMask
{
    /// <summary>Returns <c>****{last 4}</c> or <c>****</c> if too short.</summary>
    public static string MaskLastFour(string? secret)
    {
        if (string.IsNullOrEmpty(secret))
            return "****";

        var s = secret.Trim();
        if (s.Length <= 4)
            return "****";

        return $"****{s[^4..]}";
    }

    public static string TruncateForLog(string? text, int maxLength = 512)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        if (text.Length <= maxLength)
            return text;

        return text[..maxLength] + "…";
    }
}
