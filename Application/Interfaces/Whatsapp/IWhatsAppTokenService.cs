namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface IWhatsAppTokenService
{
    /// <summary>
    /// Exchanges the current token for a new long-lived one via Meta API and updates the business in the database.
    /// </summary>
    Task<string> RefreshTokenAsync(Domain.Entities.Business business);
}
