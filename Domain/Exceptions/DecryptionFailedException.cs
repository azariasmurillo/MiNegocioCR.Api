namespace MiNegocioCR.Api.Domain.Exceptions;

public class DecryptionFailedException : Exception
{
    public const string ErrorCode = "DECRYPTION_FAILED";

    public DecryptionFailedException(string message)
        : base(message)
    {
    }

    public DecryptionFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}