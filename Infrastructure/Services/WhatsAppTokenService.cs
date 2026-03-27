using System.Text.Json;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class WhatsAppTokenService : IWhatsAppTokenService
{
    private readonly IAppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppTokenService> _logger;

    public WhatsAppTokenService(
        IAppDbContext context,
        IEncryptionService encryptionService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WhatsAppTokenService> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> RefreshTokenAsync(Domain.Entities.Business business)
    {
        // Si "WhatsApp:AppId" existe pero está en "", el operador ?? NO usa Meta (solo sustituye null).
        var appId = FirstNonEmpty(_configuration["WhatsApp:AppId"], _configuration["Meta:AppId"]);
        var appSecret = FirstNonEmpty(_configuration["WhatsApp:AppSecret"], _configuration["Meta:AppSecret"]);

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
        {
            _logger.LogWarning("[WhatsApp Token] AppId o AppSecret no configurados. No se puede renovar el token.");
            throw new InvalidOperationException("WhatsApp AppId and AppSecret must be configured for token refresh.");
        }

        if (string.IsNullOrEmpty(business.WhatsappAccessToken))
        {
            _logger.LogWarning("[WhatsApp Token] El negocio {BusinessId} no tiene token configurado.", business.Id);
            throw new InvalidOperationException("Business has no WhatsApp token to refresh.");
        }

        _logger.LogInformation("[WhatsApp Token] Token refresh iniciado para negocio {BusinessId}.", business.Id);

        string currentToken;
        try
        {
            currentToken = _encryptionService.Decrypt(business.WhatsappAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WhatsApp Token] Error al desencriptar el token del negocio {BusinessId}.", business.Id);
            throw;
        }

        var url = "https://graph.facebook.com/v19.0/oauth/access_token" +
            $"?grant_type=fb_exchange_token" +
            $"&client_id={Uri.EscapeDataString(appId)}" +
            $"&client_secret={Uri.EscapeDataString(appSecret)}" +
            $"&fb_exchange_token={Uri.EscapeDataString(currentToken)}";

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            var isSessionExpired = errorBody.Contains("Session has expired", StringComparison.OrdinalIgnoreCase)
                || errorBody.Contains("error_subcode\":463", StringComparison.Ordinal);

            if (isSessionExpired)
            {
                _logger.LogWarning(
                    "[WhatsApp Token] Token ya expirado (session expired). No se puede canjear. Negocio {BusinessId} debe reconectar WhatsApp. Response: {Response}",
                    business.Id, errorBody);
                throw new InvalidOperationException(
                    "WhatsApp token has expired and cannot be refreshed. The business must reconnect WhatsApp from the app.");
            }

            _logger.LogError("[WhatsApp Token] Token refresh fallido. Status: {Status}, Response: {Response}",
                response.StatusCode, errorBody);
            throw new InvalidOperationException($"Meta token exchange failed: {response.StatusCode}. {errorBody}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("access_token", out var accessTokenEl) ||
            !root.TryGetProperty("expires_in", out var expiresInEl))
        {
            _logger.LogError("[WhatsApp Token] Respuesta de Meta sin access_token o expires_in: {Json}", json);
            throw new InvalidOperationException("Invalid response from Meta: missing access_token or expires_in.");
        }

        var newToken = accessTokenEl.GetString() ?? "";
        var expiresIn = expiresInEl.TryGetInt32(out var sec) ? sec : 0;
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

        business.WhatsappAccessToken = _encryptionService.Encrypt(newToken);
        business.WhatsappTokenExpiresAt = expiresAt;
        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "[WhatsApp Token] Token renovado exitosamente para negocio {BusinessId}. Expira: {ExpiresAt}.",
            business.Id, expiresAt);

        return newToken;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                return v;
        }
        return null;
    }
}
