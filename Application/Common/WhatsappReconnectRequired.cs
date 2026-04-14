namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Returned when the stored WhatsApp user token is past <c>WhatsappTokenExpiresAt</c> (from Meta <c>expires_in</c>).
/// System User tokens use null expiry and are not validated by this code path.
/// </summary>
public static class WhatsappReconnectRequired
{
    public const string Code = "WHATSAPP_RECONNECT_REQUIRED";
}
