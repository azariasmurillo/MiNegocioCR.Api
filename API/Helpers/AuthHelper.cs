using System.Security.Claims;

namespace MiNegocioCR.Api.API.Helpers;

public static class AuthHelper
{
    public static Guid? GetUserId(HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static Guid? GetBusinessId(HttpContext context)
    {
        var value = context.User.FindFirstValue("businessId")
                    ?? context.User.FindFirstValue("business_id");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
