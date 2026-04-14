namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>
/// Result of Meta <c>grant_type=fb_exchange_token</c> (user short/long-lived exchange).
/// </summary>
public sealed class WhatsAppTokenExchangeResult
{
    public bool Succeeded { get; init; }
    public string? LongLivedAccessToken { get; init; }
    /// <summary>Seconds until expiry from Meta (<c>expires_in</c>).</summary>
    public int? ExpiresInSeconds { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    /// <summary>Meta reports session/token expired; token cannot be exchanged.</summary>
    public bool SessionExpired { get; init; }
    /// <summary>AppId/AppSecret not configured; exchange was not attempted.</summary>
    public bool AppCredentialsMissing { get; init; }
    public string? ErrorBody { get; init; }
}
