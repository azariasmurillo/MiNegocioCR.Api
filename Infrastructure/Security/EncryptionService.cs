using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Infrastructure.Security
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("MiNegocioCR.WhatsappTokens");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new EncryptionFailedException("Plain text cannot be null or empty.");

            try
            {
                return _protector.Protect(plainText);
            }
            catch (CryptographicException ex)
            {
                throw new EncryptionFailedException("Encryption failed.", ex);
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new DecryptionFailedException("Cipher text cannot be null or empty.");

            try
            {
                return _protector.Unprotect(cipherText);
            }
            catch (CryptographicException)
            {
                throw new DecryptionFailedException(
                    "Decryption failed. The data may be corrupted, tampered, or was encrypted with a different key or purpose.");
            }
        }
    }
}
