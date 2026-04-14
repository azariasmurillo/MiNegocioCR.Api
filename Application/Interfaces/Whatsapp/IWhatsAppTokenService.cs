using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface IWhatsAppTokenService
{
    /// <summary>
    /// Meta <c>fb_exchange_token</c>: converts a valid user access token to a long-lived token (or refreshes before expiry).
    /// Does not update the database.
    /// </summary>
    Task<WhatsAppTokenExchangeResult> ExchangeUserTokenAsync(string plainTextAccessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges the stored encrypted token for a new long-lived one and updates the business row.
    /// </summary>
    Task<string> RefreshTokenAsync(global::MiNegocioCR.Api.Domain.Entities.Business business);
}
