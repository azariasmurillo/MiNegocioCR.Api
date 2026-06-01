namespace MiNegocioCR.Api.Application.Common;

public static class CampaignPersonalization
{
    public const string NamePlaceholder = "{nombre}";

    public static string ResolveFirstName(string? contactName)
    {
        if (string.IsNullOrWhiteSpace(contactName))
            return "cliente";

        var trimmed = contactName.Trim();
        var first = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? "cliente" : first;
    }

    public static string ApplySubjectTemplate(string subjectTemplate, string? contactName)
    {
        if (string.IsNullOrWhiteSpace(subjectTemplate))
            return string.Empty;

        var firstName = ResolveFirstName(contactName);
        if (subjectTemplate.Contains(NamePlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            return subjectTemplate.Replace(NamePlaceholder, firstName, StringComparison.OrdinalIgnoreCase).Trim();
        }

        return $"{firstName}, {subjectTemplate.Trim()}";
    }

    public static string ApplyBodyTemplate(string? bodyText, string? contactName)
    {
        if (string.IsNullOrWhiteSpace(bodyText))
            return string.Empty;

        var firstName = ResolveFirstName(contactName);
        if (bodyText.Contains(NamePlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            return bodyText.Replace(NamePlaceholder, firstName, StringComparison.OrdinalIgnoreCase);
        }

        return bodyText;
    }
}
