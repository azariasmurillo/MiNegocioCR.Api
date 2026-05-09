using System.Security.Cryptography;
using System.Text;

namespace MiNegocioCR.Api.Application.Common;

public static class PasswordResetTokenCrypto
{
    public static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Hash estable para persistir en BD (hex minúsculas).</summary>
    public static string HashRawToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
