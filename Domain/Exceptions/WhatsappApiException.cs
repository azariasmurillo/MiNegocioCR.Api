namespace MiNegocioCR.Api.Domain.Exceptions;

/// <summary>
/// Error response from Meta WhatsApp / Graph API (JSON <c>error</c> object).
/// Meta <c>error.message</c> is exposed as <see cref="Exception.Message"/>.
/// </summary>
public class WhatsappApiException : Exception
{
    /// <summary>Meta <c>error.code</c> (e.g. 190 OAuthException).</summary>
    public int? Code { get; }

    /// <summary>Meta <c>error.error_subcode</c> (e.g. 463 session expired).</summary>
    public int? Subcode { get; }

    /// <summary>Meta <c>error.type</c>.</summary>
    public string? Type { get; }

    /// <summary>Meta <c>error.message</c> (same as <see cref="Exception.Message"/>).</summary>
    public new string Message => base.Message;

    /// <summary>Raw JSON body for diagnostics.</summary>
    public string? RawResponse { get; }

    public WhatsappApiException(
        string message,
        int? code,
        int? subcode,
        string? type = null,
        string? rawResponse = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Subcode = subcode;
        Type = type;
        RawResponse = rawResponse;
    }
}
