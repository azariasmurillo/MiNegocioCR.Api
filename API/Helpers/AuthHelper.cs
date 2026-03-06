using Microsoft.AspNetCore.Http;

namespace MiNegocioCR.Api.API.Helpers
{    
    public static class AuthHelper
    {
        public static string? GetFirebaseUid(HttpContext context)
        {
            return context.Items["FirebaseUid"]?.ToString();
        }
    }
}
