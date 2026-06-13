using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MiNegocioCR.Api.Application.Common
{
    public static class CatalogDimensionRules
    {
        public const int MaxDimensionsPerProduct = 3;

        public const string CustomDimensionToken = "__custom__";

        public static readonly IReadOnlyList<string> StandardDimensionNames = new[]
        {
            "Marca",
            "Color",
            "Talla",
            "Capacidad",
            "Presentación",
            "Modelo",
            "Compatibilidad",
            "Material",
            "Sabor",
            "Tamaño",
        };

        public static bool IsStandardDimension(string? name)
        {
            return TryResolveStandardName(name, out _);
        }

        public static bool TryResolveStandardName(string? name, out string canonical)
        {
            canonical = string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var trimmed = name.Trim();
            foreach (var standard in StandardDimensionNames)
            {
                if (string.Equals(trimmed, standard, StringComparison.OrdinalIgnoreCase))
                {
                    canonical = standard;
                    return true;
                }
            }

            return false;
        }

        public static string ValidateAndNormalizeDimensionName(string? name, bool isCustomDimension)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre de la dimensión es obligatorio.", nameof(name));
            }

            if (TryResolveStandardName(name, out var canonical))
            {
                if (isCustomDimension)
                {
                    throw new ArgumentException(
                        "Las dimensiones estándar deben elegirse del catálogo, no como personalizadas.",
                        nameof(name));
                }

                return canonical;
            }

            if (!isCustomDimension)
            {
                throw new ArgumentException(
                    "Elegí una dimensión del catálogo o marcá la opción como personalizada.",
                    nameof(name));
            }

            var custom = NormalizeCustomDimensionName(name);
            if (custom.Length < 2)
            {
                throw new ArgumentException("La dimensión personalizada debe tener al menos 2 caracteres.", nameof(name));
            }

            if (custom.Length > 80)
            {
                throw new ArgumentException("La dimensión personalizada no puede superar 80 caracteres.", nameof(name));
            }

            return custom;
        }

        public static string NormalizeCustomDimensionName(string name)
        {
            var trimmed = CollapseWhitespace(name.Trim());
            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            return CultureInfo.GetCultureInfo("es-CR").TextInfo.ToTitleCase(trimmed.ToLowerInvariant());
        }

        public static string NormalizeDimensionValue(string? raw, string dimensionName)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ArgumentException("El valor es obligatorio.", nameof(raw));
            }

            var trimmed = CollapseWhitespace(raw.Trim());
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("El valor es obligatorio.", nameof(raw));
            }

            if (trimmed.Length > 120)
            {
                throw new ArgumentException("El valor no puede superar 120 caracteres.", nameof(raw));
            }

            if (TryResolveStandardName(dimensionName, out var canonicalDimension))
            {
                return canonicalDimension switch
                {
                    "Talla" => NormalizeSizeValue(trimmed),
                    "Capacidad" => NormalizeCapacityValue(trimmed),
                    "Compatibilidad" => NormalizeCompatibilityValue(trimmed),
                    "Marca" => NormalizeBrandValue(trimmed),
                    _ => NormalizeTitleValue(trimmed),
                };
            }

            return NormalizeTitleValue(trimmed);
        }

        public static string BuildValueKey(string dimensionName, string normalizedValue)
        {
            var dim = dimensionName.Trim().ToLowerInvariant();
            var val = normalizedValue.Trim().ToLowerInvariant();
            return $"{dim}|{val}";
        }

        private static string NormalizeSizeValue(string value)
        {
            var upper = value.ToUpperInvariant();
            return upper switch
            {
                "XS" or "S" or "M" or "L" or "XL" or "XXL" or "XXXL" => upper,
                "UNICA" or "ÚNICA" or "UNICO" or "ÚNICO" => "Única",
                _ => NormalizeTitleValue(value),
            };
        }

        private static string NormalizeCapacityValue(string value)
        {
            var match = Regex.Match(
                value.Replace(" ", string.Empty),
                @"^(?<num>\d+(?:\.\d+)?)(?<unit>GB|TB|MB|ML|L|G|KG|MAH)$",
                RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return NormalizeTitleValue(value);
            }

            var num = match.Groups["num"].Value;
            var unit = match.Groups["unit"].Value.ToUpperInvariant() switch
            {
                "G" => "GB",
                "L" when num.Contains('.') => "L",
                "ML" => "ml",
                _ => match.Groups["unit"].Value.ToUpperInvariant(),
            };

            return $"{num} {unit}";
        }

        private static string NormalizeCompatibilityValue(string value)
        {
            var known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["USB-A"] = "USB-A",
                ["USBA"] = "USB-A",
                ["USB-C"] = "USB-C",
                ["USBC"] = "USB-C",
                ["LIGHTNING"] = "Lightning",
                ["MICRO USB"] = "Micro USB",
                ["MICRO-USB"] = "Micro USB",
                ["ANDROID"] = "Android",
                ["IPHONE"] = "iPhone",
                ["WINDOWS"] = "Windows",
                ["MAC"] = "Mac",
                ["LINUX"] = "Linux",
            };

            return known.TryGetValue(value, out var canonical) ? canonical : NormalizeTitleValue(value);
        }

        private static string NormalizeBrandValue(string value)
        {
            if (value.Length <= 4 && value.All(char.IsLetter))
            {
                return value.ToUpperInvariant();
            }

            return NormalizeTitleValue(value);
        }

        private static string NormalizeTitleValue(string value)
        {
            return CultureInfo.GetCultureInfo("es-CR").TextInfo.ToTitleCase(value.ToLowerInvariant());
        }

        private static string CollapseWhitespace(string value)
        {
            var sb = new StringBuilder(value.Length);
            var previousSpace = false;
            foreach (var ch in value)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!previousSpace)
                    {
                        sb.Append(' ');
                        previousSpace = true;
                    }

                    continue;
                }

                previousSpace = false;
                sb.Append(ch);
            }

            return sb.ToString().Trim();
        }
    }
}
