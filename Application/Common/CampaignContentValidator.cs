namespace MiNegocioCR.Api.Application.Common;

public static class CampaignContentValidator
{
    /// <summary>Caracteres útiles mínimos en asunto (sin contar solo «{nombre}»).</summary>
    public const int MinSubjectSubstantiveLength = 8;

    /// <summary>Caracteres útiles mínimos en cuerpo cuando no hay imagen.</summary>
    public const int MinBodySubstantiveLength = 25;

    /// <summary>Caracteres útiles mínimos en cuerpo cuando hay imagen de apoyo.</summary>
    public const int MinBodyWithImageSubstantiveLength = 10;

    public static void ValidateSubject(string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("El asunto es obligatorio.");

        var substantive = GetSubstantiveText(subject);
        if (substantive.Length < MinSubjectSubstantiveLength)
        {
            throw new ArgumentException(
                $"El asunto debe tener al menos {MinSubjectSubstantiveLength} caracteres útiles " +
                $"(sin contar solo «{CampaignPersonalization.NamePlaceholder}»). " +
                "Ej: «{nombre}, promoción de fin de año».");
        }
    }

    public static void ValidateContent(string? bodyText, string? imageUrl)
    {
        var hasBody = !string.IsNullOrWhiteSpace(bodyText);
        var hasImage = !string.IsNullOrWhiteSpace(imageUrl);

        if (!hasBody && !hasImage)
            throw new ArgumentException("Se requiere texto o imagen para la campaña.");

        if (!hasBody)
            return;

        var substantive = GetSubstantiveText(bodyText!);
        var minLength = hasImage ? MinBodyWithImageSubstantiveLength : MinBodySubstantiveLength;
        if (substantive.Length < minLength)
        {
            throw new ArgumentException(
                $"El texto del correo es muy corto (mínimo {minLength} caracteres útiles). " +
                "Agregá más contexto: oferta, fecha o llamado a la acción.");
        }
    }

    public static void Validate(string? subject, string? bodyText, string? imageUrl)
    {
        ValidateSubject(subject);
        ValidateContent(bodyText, imageUrl);
    }

    public static string GetSubstantiveText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var withoutPlaceholder = text
            .Replace(CampaignPersonalization.NamePlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase);

        return withoutPlaceholder.Trim().Trim(',', '.', ':', ';', '!', '?', ' ', '-').Trim();
    }
}
