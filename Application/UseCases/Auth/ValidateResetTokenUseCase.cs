using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;

namespace MiNegocioCR.Api.Application.UseCases.Auth;

public class ValidateResetTokenUseCase : IValidateResetTokenUseCase
{
    private readonly IPasswordResetTokenRepository _tokenRepository;

    public ValidateResetTokenUseCase(IPasswordResetTokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<bool> ExecuteAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return false;

        var hash = PasswordResetTokenCrypto.HashRawToken(rawToken.Trim());
        return await _tokenRepository.IsActiveValidTokenAsync(hash, cancellationToken);
    }
}
