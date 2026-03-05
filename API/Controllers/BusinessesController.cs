using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly ICreateBusinessUseCase _createBusinessUseCase;
    private readonly IConfigureSmtpUseCase _configureSmtpUseCase;
    private readonly ISetBusinessActiveStatusUseCase _setBusinessActiveStatusUseCase;
    private readonly IGetBusinessByIdUseCase _getBusinessByIdUseCase;

    public BusinessesController(
        ICreateBusinessUseCase createBusinessUseCase,
        IConfigureSmtpUseCase configureSmtpUseCase,
        ISetBusinessActiveStatusUseCase setBusinessActiveStatusUseCase,
        IGetBusinessByIdUseCase getBusinessByIdUseCase  )
    {
        _createBusinessUseCase = createBusinessUseCase;
        _configureSmtpUseCase = configureSmtpUseCase;
        _setBusinessActiveStatusUseCase = setBusinessActiveStatusUseCase;
        _getBusinessByIdUseCase = getBusinessByIdUseCase;
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

}