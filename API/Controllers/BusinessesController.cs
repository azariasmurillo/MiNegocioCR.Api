using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Filters;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Business;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly ICreateBusinessUseCase _createBusinessUseCase;
    private readonly IConfigureSmtpUseCase _configureSmtpUseCase;
    private readonly ISetBusinessActiveStatusUseCase _setBusinessActiveStatusUseCase;
    private readonly IGetBusinessByIdUseCase _getBusinessByIdUseCase;
    private readonly IBusinessRepository _businessRepository;
    private readonly IEmailService _emailService;
    private readonly ISetEnableAIChatUseCase _setEnableAIChatUseCase;
    private readonly IGetBusinessConfigUseCase _getBusinessConfigUseCase;
    private readonly IUpdateBusinessConfigUseCase _updateBusinessConfigUseCase;
    private readonly IUploadBusinessLogoUseCase _uploadBusinessLogoUseCase;

    public BusinessesController(
        ICreateBusinessUseCase createBusinessUseCase,
        IConfigureSmtpUseCase configureSmtpUseCase,
        ISetBusinessActiveStatusUseCase setBusinessActiveStatusUseCase,
        IGetBusinessByIdUseCase getBusinessByIdUseCase,
        IBusinessRepository businessRepository,
        IEmailService emailService,
        ISetEnableAIChatUseCase setEnableAIChatUseCase,
        IGetBusinessConfigUseCase getBusinessConfigUseCase,
        IUpdateBusinessConfigUseCase updateBusinessConfigUseCase,
        IUploadBusinessLogoUseCase uploadBusinessLogoUseCase)
    {
        _createBusinessUseCase = createBusinessUseCase;
        _configureSmtpUseCase = configureSmtpUseCase;
        _setBusinessActiveStatusUseCase = setBusinessActiveStatusUseCase;
        _getBusinessByIdUseCase = getBusinessByIdUseCase;
        _businessRepository = businessRepository;
        _emailService = emailService;
        _setEnableAIChatUseCase = setEnableAIChatUseCase;
        _getBusinessConfigUseCase = getBusinessConfigUseCase;
        _updateBusinessConfigUseCase = updateBusinessConfigUseCase;
        _uploadBusinessLogoUseCase = uploadBusinessLogoUseCase;
    }

    [HttpPost]
    [AdminOnly]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequestDto request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        var result = await _createBusinessUseCase.Execute(request);
        return Ok(result);
    }

    [HttpPut("{businessId}/smtp")]
    public async Task<IActionResult> ConfigureSmtp(Guid businessId, [FromBody] ConfigureSmtpDto dto)
    {
        if (dto == null)
            return BadRequest("Request body is required.");

        await _configureSmtpUseCase.Execute(businessId, dto);
        return Ok("SMTP configured successfully");
    }

    [HttpPatch("{businessId}/status")]
    [AdminOnly]
    public async Task<IActionResult> SetStatus(Guid businessId, [FromQuery] bool isActive)
    {
        await _setBusinessActiveStatusUseCase.Execute(businessId, isActive);
        return Ok($"Business status updated to {(isActive ? "Active" : "Inactive")}");
    }

    [HttpGet("{businessId}")]
    public async Task<IActionResult> GetById(Guid businessId)
    {
        var result = await _getBusinessByIdUseCase.Execute(businessId);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{businessId}/test-email")]
    public async Task<IActionResult> SendTestEmail(
    Guid businessId,
    [FromBody] TestEmailRequest request)
    {
        var business = await _businessRepository.GetByIdAsync(businessId);

        if (business == null)
            return NotFound("Business not found");

        if (string.IsNullOrEmpty(business.SmtpHost))
            return BadRequest("SMTP is not configured");

        await _emailService.SendAsync(
            business,
            request.Email,
            "SMTP Test - Mi-NegocioCR",
            "<h2>SMTP configurado correctamente</h2><p>Tu correo est? funcionando.</p>");

        return Ok(new { message = "Test email sent successfully" });
    }

    [HttpPatch("{businessId}/ai-chat")]
    public async Task<IActionResult> SetEnableAIChat(Guid businessId, [FromBody] SetEnableAIChatDto dto)
    {
        if (dto == null)
            return BadRequest("Request body is required.");

        await _setEnableAIChatUseCase.ExecuteAsync(businessId, dto.Enable);
        return Ok(new { enableAIChat = dto.Enable });
    }

    [HttpGet("/api/business/{businessId:guid}/config")]
    public async Task<IActionResult> GetConfig(Guid businessId)
    {
        var result = await _getBusinessConfigUseCase.Execute(businessId);
        if (result == null) return NotFound("Business not found");
        return Ok(result);
    }

    [HttpPut("/api/business/{businessId:guid}/config")]
    public async Task<IActionResult> UpdateConfig(Guid businessId, [FromBody] UpdateBusinessConfigRequestDto request)
    {
        if (request == null) return BadRequest("Request body is required.");
        var result = await _updateBusinessConfigUseCase.Execute(businessId, request);
        return Ok(result);
    }

    [HttpPost("/api/business/{businessId:guid}/logo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(
        Guid businessId,
        [FromForm] UploadBusinessLogoRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("Logo file is required.");
        if (request.File.Length > 2 * 1024 * 1024)
            return BadRequest("Logo file must be less than 2MB.");
        if (!IsAllowedLogoContentType(request.File.ContentType))
            return BadRequest("Only PNG/JPG files are allowed.");

        await using var stream = request.File.OpenReadStream();
        var logoUrl = await _uploadBusinessLogoUseCase.Execute(
            businessId,
            stream,
            request.File.FileName);

        return Ok(new { logoUrl });
    }

    public sealed class UploadBusinessLogoRequest
    {
        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }
    }

    private static bool IsAllowedLogoContentType(string? contentType)
    {
        return string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase);
    }

}