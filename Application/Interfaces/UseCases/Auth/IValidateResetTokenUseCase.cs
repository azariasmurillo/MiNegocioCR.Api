namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;

public interface IValidateResetTokenUseCase
{
    /// <summary>Token crudo del query string; valida existencia, no usado y no expirado.</summary>
    Task<bool> ExecuteAsync(string rawToken, CancellationToken cancellationToken = default);
}
