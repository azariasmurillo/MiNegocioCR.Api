using Microsoft.AspNetCore.DataProtection;

namespace MiNegocioCR.Api.API.Services;

public interface IAdminAuthService
{
    bool ValidatePassword(string password);
    string CreateAuthCookieValue();
    bool ValidateAuthCookie(string? cookieValue);
}

public class AdminAuthService : IAdminAuthService
{
    public const string CookieName = "AdminSession";
    private const string Purpose = "MiNegocioCR.Admin.Auth";
    private readonly IDataProtectionProvider _dataProtection;
    private readonly string _adminPassword;

    public AdminAuthService(IDataProtectionProvider dataProtection, IConfiguration configuration)
    {
        _dataProtection = dataProtection;
        _adminPassword = configuration["Admin:Password"] ?? "AzaMur542431@";
    }

    public bool ValidatePassword(string password) => password == _adminPassword;

    public string CreateAuthCookieValue()
    {
        var protector = _dataProtection.CreateProtector(Purpose);
        var payload = $"admin|{DateTime.UtcNow.AddHours(24):O}";
        return protector.Protect(payload);
    }

    public bool ValidateAuthCookie(string? cookieValue)
    {
        if (string.IsNullOrEmpty(cookieValue)) return false;
        try
        {
            var protector = _dataProtection.CreateProtector(Purpose);
            var payload = protector.Unprotect(cookieValue);
            var parts = payload.Split('|');
            if (parts.Length != 2) return false;
            if (parts[0] != "admin") return false;
            return DateTime.TryParse(parts[1], out var expiry) && expiry > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }
}
