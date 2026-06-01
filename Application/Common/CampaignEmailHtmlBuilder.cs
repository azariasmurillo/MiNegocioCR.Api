using System.Net;

namespace MiNegocioCR.Api.Application.Common;

public static class CampaignEmailHtmlBuilder
{
    public static string Build(
        string businessName,
        string? logoUrl,
        string? bodyText,
        string? imageUrl,
        string? contactName = null)
    {
        var safeName = WebUtility.HtmlEncode(businessName);
        var parts = new List<string>();

        parts.Add("""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:640px;margin:0 auto;color:#0f172a;">
            """);

        if (!string.IsNullOrWhiteSpace(logoUrl))
        {
            var safeLogo = WebUtility.HtmlEncode(logoUrl.Trim());
            parts.Add($"""<div style="text-align:center;margin-bottom:20px;"><img src="{safeLogo}" alt="{safeName}" style="max-height:72px;max-width:220px;" /></div>""");
        }

        parts.Add($"""<div style="font-size:14px;line-height:1.6;">""");

        var personalizedBody = CampaignPersonalization.ApplyBodyTemplate(bodyText, contactName);
        if (!string.IsNullOrWhiteSpace(personalizedBody))
        {
            var paragraphs = personalizedBody
                .Replace("\r\n", "\n")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var paragraph in paragraphs)
                parts.Add($"<p style=\"margin:0 0 14px;\">{WebUtility.HtmlEncode(paragraph)}</p>");
        }
        else if (!string.IsNullOrWhiteSpace(contactName))
        {
            var greeting = WebUtility.HtmlEncode($"Hola {CampaignPersonalization.ResolveFirstName(contactName)},");
            parts.Add($"<p style=\"margin:0 0 14px;\">{greeting}</p>");
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            var safeImage = WebUtility.HtmlEncode(imageUrl.Trim());
            parts.Add(
                $"""
                <p style="margin:16px 0;text-align:center;">
                  <img src="{safeImage}" alt="Promoción {safeName}" width="{CampaignImageLimits.MaxDisplayWidth}" style="max-width:100%;width:100%;height:auto;display:block;border:0;border-radius:8px;" />
                </p>
                """);
        }

        parts.Add("</div>");
        parts.Add($"""<p style="margin-top:24px;font-size:12px;color:#64748b;">{safeName}</p>""");
        parts.Add("</div>");

        return string.Concat(parts);
    }

    public static void ValidateContent(string? bodyText, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(bodyText) && string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Se requiere texto o imagen para la campaña.");
    }
}
