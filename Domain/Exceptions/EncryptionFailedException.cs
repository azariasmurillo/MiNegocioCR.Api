namespace MiNegocioCR.Api.Domain.Exceptions
{    
    public class EncryptionFailedException : Exception
    {
        public const string ErrorCode = "ENCRYPTION_FAILED";

        public EncryptionFailedException(string message)
            : base(message)
        {
        }

        public EncryptionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
