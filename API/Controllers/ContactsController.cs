using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IListContactsUseCase _listContactsUseCase;
    private readonly ISearchContactsUseCase _searchContactsUseCase;
    private readonly IUpdateContactUseCase _updateContactUseCase;
    private readonly ISoftDeleteContactUseCase _softDeleteContactUseCase;
    private readonly IHardDeleteContactUseCase _hardDeleteContactUseCase;

    public ContactsController(
        IListContactsUseCase listContactsUseCase,
        ISearchContactsUseCase searchContactsUseCase,
        IUpdateContactUseCase updateContactUseCase,
        ISoftDeleteContactUseCase softDeleteContactUseCase,
        IHardDeleteContactUseCase hardDeleteContactUseCase)
    {
        _listContactsUseCase = listContactsUseCase;
        _searchContactsUseCase = searchContactsUseCase;
        _updateContactUseCase = updateContactUseCase;
        _softDeleteContactUseCase = softDeleteContactUseCase;
        _hardDeleteContactUseCase = hardDeleteContactUseCase;
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> List(Guid businessId)
    {
        var contacts = await _listContactsUseCase.Execute(businessId);
        return Ok(contacts);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] Guid businessId, [FromQuery] string? query)
    {
        var contacts = await _searchContactsUseCase.Execute(businessId, query);
        return Ok(contacts);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromQuery] Guid businessId,
        [FromBody] UpdateContactRequestDto request)
    {
        if (request == null) return BadRequest("ContactsUpdate - Request body is required.");
        var result = await _updateContactUseCase.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/delete")]
    public async Task<IActionResult> SoftDelete(Guid id, [FromQuery] Guid businessId)
    {
        var result = await _softDeleteContactUseCase.Execute(businessId, id);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> HardDelete(Guid id, [FromQuery] Guid businessId)
    {
        await _hardDeleteContactUseCase.Execute(businessId, id);
        return NoContent();
    }
}
