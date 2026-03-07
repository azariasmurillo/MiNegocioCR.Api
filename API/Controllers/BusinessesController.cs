using Microsoft.AspNetCore.Mvc;
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

    public BusinessesController(
        ICreateBusinessUseCase createBusinessUseCase,
        IConfigureSmtpUseCase configureSmtpUseCase,
        ISetBusinessActiveStatusUseCase setBusinessActiveStatusUseCase,
        IGetBusinessByIdUseCase getBusinessByIdUseCase,
        IBusinessRepository businessRepository,
        IEmailService emailService,
        ISetEnableAIChatUseCase setEnableAIChatUseCase)
    {
        _createBusinessUseCase = createBusinessUseCase;
        _configureSmtpUseCase = configureSmtpUseCase;
        _setBusinessActiveStatusUseCase = setBusinessActiveStatusUseCase;
        _getBusinessByIdUseCase = getBusinessByIdUseCase;
        _businessRepository = businessRepository;
        _emailService = emailService;
        _setEnableAIChatUseCase = setEnableAIChatUseCase;
    }

    [HttpPost]
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
            "<h2>SMTP configurado correctamente</h2><p>Tu correo está funcionando.</p>");

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

}