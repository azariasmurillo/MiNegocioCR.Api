using FirebaseAdmin.Auth;
using MiNegocioCR.Api.Application.Interfaces.Auth;

namespace MiNegocioCR.Api.Infrastructure.Auth
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        public async Task<string?> VerifyTokenAsync(string token)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                return decodedToken.Uid;
            }
            catch
            {
                return null;
            }
        }
    }
}
