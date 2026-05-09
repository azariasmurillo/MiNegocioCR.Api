namespace MiNegocioCR.Api.Application.DTOs;

public enum ForgotPasswordProcessStatus
{
    InvalidEmail,
    UserNotFound,
    EmailSent,
    EmailSendFailed
}

public sealed class ForgotPasswordProcessResult
{
    public ForgotPasswordProcessStatus Status { get; init; }
    public string? Error { get; init; }
}
