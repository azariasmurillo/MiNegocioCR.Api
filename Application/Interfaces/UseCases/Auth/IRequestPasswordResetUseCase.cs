using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;

public interface IRequestPasswordResetUseCase
{
    /// <summary>
    /// Si el usuario existe y el negocio puede enviar SMTP, crea token (10 min) y envía email.
    /// Si no aplica, no revela si el email existe.
    /// </summary>
    Task<ForgotPasswordProcessResult> ExecuteAsync(string email, CancellationToken cancellationToken = default);
}
