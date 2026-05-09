namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;

public interface IResetPasswordUseCase
{
    /// <summary>
    /// Valida token crudo, vigencia y uso único; guarda nuevo hash bcrypt.
    /// </summary>
    Task<bool> ExecuteAsync(string rawToken, string newPassword, CancellationToken cancellationToken = default);
}
