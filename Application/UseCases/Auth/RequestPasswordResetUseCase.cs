using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Configuration;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Auth;

public class RequestPasswordResetUseCase : IRequestPasswordResetUseCase
{
    private const int TokenLifetimeMinutes = 10;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly AppOptions _appOptions;
    private readonly ILogger<RequestPasswordResetUseCase> _logger;

    public RequestPasswordResetUseCase(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IEmailService emailService,
        IOptions<AppOptions> appOptions,
        ILogger<RequestPasswordResetUseCase> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task<ForgotPasswordProcessResult> ExecuteAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ForgotPasswordProcessResult { Status = ForgotPasswordProcessStatus.InvalidEmail };
        }

        var user = await _userRepository.GetByEmailAsync(email.Trim());
        if (user == null)
        {
            return new ForgotPasswordProcessResult { Status = ForgotPasswordProcessStatus.UserNotFound };
        }

        var raw = PasswordResetTokenCrypto.GenerateRawToken();
        var hash = PasswordResetTokenCrypto.HashRawToken(raw);

        var entity = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(entity, cancellationToken);

        var baseUrl = _appOptions.PublicUrl.TrimEnd('/');
        var link = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(raw)}";

        try
        {
            await _emailService.SendPasswordResetEmail(user.Email, link);
            _logger.LogInformation("Password reset request processed for {Email}", user.Email);
            return new ForgotPasswordProcessResult { Status = ForgotPasswordProcessStatus.EmailSent };
        }
        catch (Exception ex)
        {
            await _tokenRepository.DeleteAsync(entity.Id, cancellationToken);
            _logger.LogError(ex, "Password reset email failed for {Email}", user.Email);
            return new ForgotPasswordProcessResult
            {
                Status = ForgotPasswordProcessStatus.EmailSendFailed,
                Error = ex.Message
            };
        }
    }
}
