using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Filters;
using MiNegocioCR.Api.API.Services;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Business;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminAuthService _adminAuth;
    private readonly IBusinessRepository _businessRepository;

    public AdminController(IAdminAuthService adminAuth, IBusinessRepository businessRepository)
    {
        _adminAuth = adminAuth;
        _businessRepository = businessRepository;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest request)
    {
        if (request?.Password == null || !_adminAuth.ValidatePassword(request.Password))
            return BadRequest(new { error = "Contraseña incorrecta" });

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromHours(24),
            Path = "/"
        };
        Response.Cookies.Append(AdminAuthService.CookieName, _adminAuth.CreateAuthCookieValue(), cookieOptions);
        return Ok(new { success = true });
    }

    [HttpGet("businesses")]
    [AdminOnly]
    public async Task<IActionResult> GetBusinesses()
    {
        var list = await _businessRepository.GetAllAsync();
        var dtos = list.Select(b => new AdminBusinessListItemDto
        {
            Id = b.Id,
            Name = b.Name,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt
        }).ToList();
        return Ok(dtos);
    }
}

public class AdminLoginRequest
{
    public string? Password { get; set; }
}
