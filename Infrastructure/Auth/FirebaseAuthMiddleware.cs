using MiNegocioCR.Api.Application.Interfaces.Auth;

namespace MiNegocioCR.Api.Infrastructure.Auth
{
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public FirebaseAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IFirebaseAuthService firebaseAuth)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

                var uid = await firebaseAuth.VerifyTokenAsync(token);

                if (uid != null)
                {
                    context.Items["FirebaseUid"] = uid;
                }
            }

            await _next(context);
        }
    }
}
