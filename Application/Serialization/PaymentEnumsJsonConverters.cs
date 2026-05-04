using System.Text.Json;
using System.Text.Json.Serialization;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Serialization;

/// <summary>
/// Accepts numeric values, English enum names, and common Spanish synonyms for API requests.
/// </summary>
public sealed class PaymentTypeJsonConverter : JsonConverter<PaymentType>
{
    public override PaymentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.TryGetInt32(out var n)
                ? ToPaymentTypeOrThrow(n)
                : throw new JsonException("Invalid numeric payment type."),
            JsonTokenType.String => ParseFromString(reader.GetString()),
            _ => throw new JsonException("Payment type must be a number or string.")
        };
    }

    public override void Write(Utf8JsonWriter writer, PaymentType value, JsonSerializerOptions options)
    {
        var name = JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
        writer.WriteStringValue(name);
    }

    private static PaymentType ParseFromString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new JsonException("Payment type cannot be empty.");

        var s = raw.Trim();
        if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return ToPaymentTypeOrThrow(num);

        switch (s.ToLowerInvariant())
        {
            case "deposit":
            case "diagnostico":
            case "diagnóstico":
            case "diagnostic":
            case "diagnosis":
                return PaymentType.Deposit;
            case "advance":
            case "adelanto":
                return PaymentType.Advance;
            case "final":
                return PaymentType.Final;
        }

        if (Enum.TryParse<PaymentType>(s, ignoreCase: true, out var parsed))
            return parsed;

        throw new JsonException($"Unknown payment type: '{raw}'. Use deposit/diagnostic/diagnostico, advance/adelanto, or final (or 1–3).");
    }

    private static PaymentType ToPaymentTypeOrThrow(int n)
    {
        if (!Enum.IsDefined(typeof(PaymentType), n))
            throw new JsonException($"Invalid payment type value: {n}. Expected 1 (deposit), 2 (advance), or 3 (final).");
        return (PaymentType)n;
    }
}

/// <summary>
/// Accepts numeric values, English enum names (any casing), and common Spanish synonyms.
/// </summary>
public sealed class PaymentMethodJsonConverter : JsonConverter<PaymentMethod>
{
    public override PaymentMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.TryGetInt32(out var n)
                ? ToPaymentMethodOrThrow(n)
                : throw new JsonException("Invalid numeric payment method."),
            JsonTokenType.String => ParseFromString(reader.GetString()),
            _ => throw new JsonException("Payment method must be a number or string.")
        };
    }

    public override void Write(Utf8JsonWriter writer, PaymentMethod value, JsonSerializerOptions options)
    {
        var name = JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
        writer.WriteStringValue(name);
    }

    private static PaymentMethod ParseFromString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new JsonException("Payment method cannot be empty.");

        var s = raw.Trim();
        if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return ToPaymentMethodOrThrow(num);

        switch (s.ToLowerInvariant())
        {
            case "cash":
            case "efectivo":
                return PaymentMethod.Cash;
            case "transfer":
            case "transferencia":
                return PaymentMethod.Transfer;
            case "sinpe":
                return PaymentMethod.Sinpe;
            case "card":
            case "tarjeta":
                return PaymentMethod.Card;
        }

        if (Enum.TryParse<PaymentMethod>(s, ignoreCase: true, out var parsed))
            return parsed;

        throw new JsonException($"Unknown payment method: '{raw}'. Use cash/transfer/sinpe/card (or 1–4).");
    }

    private static PaymentMethod ToPaymentMethodOrThrow(int n)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), n))
            throw new JsonException($"Invalid payment method value: {n}. Expected 1–4.");
        return (PaymentMethod)n;
    }
}
