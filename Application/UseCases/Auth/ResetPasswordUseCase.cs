using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;

namespace MiNegocioCR.Api.Application.UseCases.Auth;

public class ResetPasswordUseCase : IResetPasswordUseCase
{
    private const int MinPasswordLength = 8;

    private readonly IPasswordResetTokenRepository _tokenRepository;

    public ResetPasswordUseCase(IPasswordResetTokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<bool> ExecuteAsync(string rawToken, string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        if (newPassword.Length < MinPasswordLength)
            return false;

        var hash = PasswordResetTokenCrypto.HashRawToken(rawToken.Trim());
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        return await _tokenRepository.TryCompletePasswordResetAsync(hash, newPasswordHash, cancellationToken);
    }
}
