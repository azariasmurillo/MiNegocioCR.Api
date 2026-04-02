using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

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

    /// <inheritdoc />
    public async Task<WhatsAppTokenExchangeResult> ExchangeUserTokenAsync(string plainTextAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plainTextAccessToken))
            throw new ArgumentException("Access token is required.", nameof(plainTextAccessToken));

        var appId = FirstNonEmpty(_configuration["WhatsApp:AppId"], _configuration["Meta:AppId"]);
        var appSecret = FirstNonEmpty(_configuration["WhatsApp:AppSecret"], _configuration["Meta:AppSecret"]);

        var httpClient = _httpClientFactory.CreateClient();
        return await WhatsAppTokenExchangeHelper.ExchangeTokenAsync(
            httpClient,
            appId ?? "",
            appSecret ?? "",
            plainTextAccessToken,
            _logger,
            cancellationToken);
    }

    public async Task<string> RefreshTokenAsync(MiNegocioCR.Api.Domain.Entities.Business business)
    {
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

        var httpClient = _httpClientFactory.CreateClient();
        var result = await WhatsAppTokenExchangeHelper.ExchangeTokenAsync(
            httpClient,
            appId,
            appSecret,
            currentToken,
            _logger,
            CancellationToken.None);

        if (result.Succeeded && result.LongLivedAccessToken != null && result.ExpiresAtUtc.HasValue)
        {
            business.WhatsappAccessToken = _encryptionService.Encrypt(result.LongLivedAccessToken);
            business.WhatsappTokenExpiresAt = result.ExpiresAtUtc;
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation(
                "[WhatsApp Token] Token renovado exitosamente para negocio {BusinessId}. Expira: {ExpiresAt}.",
                business.Id, result.ExpiresAtUtc);

            return result.LongLivedAccessToken;
        }

        if (result.SessionExpired)
        {
            _logger.LogWarning(
                "[WhatsApp Token] Token ya expirado (session expired). No se puede canjear. Negocio {BusinessId} debe reconectar WhatsApp. Response: {Response}",
                business.Id, TokenLogMask.TruncateForLog(result.ErrorBody));
            throw new InvalidOperationException(
                "WhatsApp token has expired and cannot be refreshed. The business must reconnect WhatsApp from the app.");
        }

        if (result.AppCredentialsMissing)
        {
            throw new InvalidOperationException("WhatsApp AppId and AppSecret must be configured for token refresh.");
        }

        _logger.LogError("[WhatsApp Token] Token refresh fallido. Response: {Response}", TokenLogMask.TruncateForLog(result.ErrorBody));
        throw new InvalidOperationException(
            $"Meta token exchange failed. The token may be a System User token (not exchangeable via fb_exchange_token). Response: {TokenLogMask.TruncateForLog(result.ErrorBody)}");
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
