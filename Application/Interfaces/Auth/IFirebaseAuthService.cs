namespace MiNegocioCR.Api.Application.Interfaces.Auth
{
    public interface IFirebaseAuthService
    {
        Task<string?> VerifyTokenAsync(string token);
    }
}
