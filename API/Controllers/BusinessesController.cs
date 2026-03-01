using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Aplication.DTOs;
using MiNegocioCR.Api.Aplication.Interfaces.Business;
using MiNegocioCR.Api.Aplication.UseCases.Business;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly ICreateBusinessUseCase _createBusinessUseCase;
    private readonly IConfigureSmtpUseCase _configureSmtpUseCase;
    private readonly ISetBusinessActiveStatusUseCase _setBusinessActiveStatusUseCase;

    public BusinessesController(
        ICreateBusinessUseCase createBusinessUseCase,
        IConfigureSmtpUseCase configureSmtpUseCase,
        ISetBusinessActiveStatusUseCase setBusinessActiveStatusUseCase)
    {
        _createBusinessUseCase = createBusinessUseCase;
        _configureSmtpUseCase = configureSmtpUseCase;
        _setBusinessActiveStatusUseCase = setBusinessActiveStatusUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequestDto request)
    {
        var result = await _createBusinessUseCase.Execute(request);
        return Ok(result);
    }

    [HttpPut("{businessId}/smtp")]
    public async Task<IActionResult> ConfigureSmtp(Guid businessId, ConfigureSmtpDto dto)
    {
        await _configureSmtpUseCase.Execute(businessId, dto);
        return Ok("SMTP configured successfully");
    }

    [HttpPatch("{businessId}/status")]
    public async Task<IActionResult> SetStatus(Guid businessId, [FromQuery] bool isActive)
    {
        await _setBusinessActiveStatusUseCase.Execute(businessId, isActive);
        return Ok($"Business status updated to {(isActive ? "Active" : "Inactive")}");
    }
}