namespace MiNegocioCR.Api.Application.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
