using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Infrastructure.Services;

/// <summary>
/// Meta Graph <c>GET /oauth/access_token?grant_type=fb_exchange_token</c> (user token → long-lived).
/// </summary>
public static class WhatsAppTokenExchangeHelper
{
    public const string DefaultGraphOAuthAccessTokenUrl = "https://graph.facebook.com/v19.0/oauth/access_token";

    /// <summary>
    /// Exchanges a user access token for a long-lived token. Does not throw on HTTP errors; inspect <see cref="WhatsAppTokenExchangeResult.Succeeded"/>.
    /// </summary>
    public static async Task<WhatsAppTokenExchangeResult> ExchangeTokenAsync(
        HttpClient httpClient,
        string appId,
        string appSecret,
        string fbExchangeToken,
        ILogger? logger,
        CancellationToken cancellationToken = default,
        string graphOAuthUrl = DefaultGraphOAuthAccessTokenUrl)
    {
        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
        {
            return new WhatsAppTokenExchangeResult
            {
                AppCredentialsMissing = true,
                ErrorBody = "AppId/AppSecret missing"
            };
        }

        if (string.IsNullOrWhiteSpace(fbExchangeToken))
            throw new ArgumentException("fb_exchange_token is required.", nameof(fbExchangeToken));

        var url = $"{graphOAuthUrl.TrimEnd('/')}" +
                  "?grant_type=fb_exchange_token" +
                  $"&client_id={Uri.EscapeDataString(appId)}" +
                  $"&client_secret={Uri.EscapeDataString(appSecret)}" +
                  $"&fb_exchange_token={Uri.EscapeDataString(fbExchangeToken)}";

        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var isSessionExpired = errorBody.Contains("Session has expired", StringComparison.OrdinalIgnoreCase)
                                   || errorBody.Contains("error_subcode\":463", StringComparison.Ordinal);

            if (isSessionExpired)
            {
                return new WhatsAppTokenExchangeResult
                {
                    SessionExpired = true,
                    ErrorBody = errorBody
                };
            }

            return new WhatsAppTokenExchangeResult
            {
                ErrorBody = errorBody
            };
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("access_token", out var accessTokenEl) ||
            !root.TryGetProperty("expires_in", out var expiresInEl))
        {
            var propNames = new StringBuilder();
            foreach (var p in root.EnumerateObject())
            {
                if (propNames.Length > 0)
                    propNames.Append(',');
                propNames.Append(p.Name);
            }

            logger?.LogError(
                "[WhatsApp Token] Respuesta de Meta sin access_token o expires_in. Root properties: {Properties}, length: {Length}",
                propNames.ToString(),
                json.Length);
            return new WhatsAppTokenExchangeResult { ErrorBody = json };
        }

        var newToken = accessTokenEl.GetString() ?? "";
        var expiresIn = expiresInEl.TryGetInt32(out var sec) ? sec : 0;
        var expiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn);

        return new WhatsAppTokenExchangeResult
        {
            Succeeded = true,
            LongLivedAccessToken = newToken,
            ExpiresInSeconds = expiresIn,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
