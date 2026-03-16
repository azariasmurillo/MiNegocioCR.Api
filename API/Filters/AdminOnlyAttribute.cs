using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiNegocioCR.Api.API.Services;

namespace MiNegocioCR.Api.API.Filters;

public class AdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authService = context.HttpContext.RequestServices.GetService<IAdminAuthService>();
        if (authService == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var cookie = context.HttpContext.Request.Cookies[authService.CookieName];
        if (!authService.ValidateAuthCookie(cookie))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await Task.CompletedTask;
    }
}
