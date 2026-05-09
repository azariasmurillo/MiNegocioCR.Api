using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Auth;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRequestPasswordResetUseCase _requestPasswordResetUseCase;
    private readonly IValidateResetTokenUseCase _validateResetTokenUseCase;
    private readonly IResetPasswordUseCase _resetPasswordUseCase;
    private readonly IEmailService _emailService;
    private readonly IHostEnvironment _environment;

    public AuthController(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IRequestPasswordResetUseCase requestPasswordResetUseCase,
        IValidateResetTokenUseCase validateResetTokenUseCase,
        IResetPasswordUseCase resetPasswordUseCase,
        IEmailService emailService,
        IHostEnvironment environment)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _requestPasswordResetUseCase = requestPasswordResetUseCase;
        _validateResetTokenUseCase = validateResetTokenUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _emailService = emailService;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto? request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { sessionMessage = "Enviá email y contraseña en el cuerpo de la petición." });
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new { sessionMessage = "Credenciales incorrectas." });
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Unauthorized(new { sessionMessage = "Credenciales incorrectas." });
        }

        bool passwordOk;
        try
        {
            passwordOk = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            passwordOk = false;
        }

        if (!passwordOk)
        {
            return Unauthorized(new { sessionMessage = "Credenciales incorrectas." });
        }

        if (!user.IsActive)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { sessionMessage = "Tu usuario está inactivo. Pedí ayuda al administrador del negocio." });
        }
        
        if (user.BusinessId == Guid.Empty)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { sessionMessage = "El usuario no tiene negocio asignado." });
        }

        var token = _jwtTokenService.CreateToken(user);
        return Ok(AuthSessionMapper.ToDto(user, token));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { sessionMessage = "Token inválido." });
        }

        var user = await _userRepository.GetByIdWithBusinessAsync(userId);

        if (user == null)
        {
            return NotFound(new { sessionMessage = "No se encontró el usuario." });
        }
        
        var businessIdClaim =
            User.FindFirstValue("businessId") ??
            User.FindFirstValue("business_id");
        if (!Guid.TryParse(businessIdClaim, out var businessIdFromToken))
        {
            return Unauthorized(new { sessionMessage = "Token inválido." });
        }
        
        if (businessIdFromToken == Guid.Empty || businessIdFromToken != user.BusinessId)
        {
            return Unauthorized(new { sessionMessage = "Token inválido para este negocio." });
        }

        var token = GetBearerToken(Request);
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { sessionMessage = "Falta el token JWT en Authorization: Bearer." });
        }

        return Ok(AuthSessionMapper.ToDto(user, token));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto? request)
    {
        var result = await _requestPasswordResetUseCase
            .ExecuteAsync(request?.Email ?? string.Empty, HttpContext.RequestAborted);

        if (_environment.IsDevelopment())
        {
            return result.Status switch
            {
                ForgotPasswordProcessStatus.InvalidEmail =>
                    BadRequest(new { success = false, message = "Email requerido.", detail = result.Status.ToString() }),
                ForgotPasswordProcessStatus.EmailSendFailed =>
                    StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        success = false,
                        message = "Error enviando correo SMTP.",
                        detail = result.Error,
                        status = result.Status.ToString()
                    }),
                ForgotPasswordProcessStatus.UserNotFound =>
                    Ok(new { success = true, message = "Usuario no encontrado.", status = result.Status.ToString() }),
                _ =>
                    Ok(new { success = true, message = "Correo enviado.", status = result.Status.ToString() })
            };
        }

        // Producción: respuesta uniforme para no filtrar existencia de cuentas.
        return Ok(new { success = true });
    }

    [AllowAnonymous]
    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest? request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return BadRequest(new { success = false, message = "Email requerido." });
        }

        try
        {
            await _emailService.SendTestEmail(request.Email.Trim());
            return Ok(new { success = true, message = "Correo de prueba enviado." });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Error enviando correo de prueba SMTP.",
                detail = _environment.IsDevelopment() ? ex.ToString() : "SMTP test failed."
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("validate-reset-token")]
    public async Task<IActionResult> ValidateResetToken([FromQuery] string? token)
    {
        var isValid = await _validateResetTokenUseCase.ExecuteAsync(token ?? string.Empty, HttpContext.RequestAborted);
        return Ok(new { isValid });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto? request)
    {
        if (string.IsNullOrWhiteSpace(request?.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { success = false });
        }

        var ok = await _resetPasswordUseCase.ExecuteAsync(request.Token, request.NewPassword, HttpContext.RequestAborted);
        if (!ok)
        {
            return BadRequest(new { success = false });
        }

        return Ok(new { success = true });
    }

    private static string? GetBearerToken(HttpRequest request)
    {
        var auth = request.Headers.Authorization.FirstOrDefault();
        if (auth != null && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return auth["Bearer ".Length..].Trim();
        }

        return null;
    }
}
