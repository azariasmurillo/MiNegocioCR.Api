using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.Common;
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
    private readonly IListContactInsightsUseCase _listContactInsightsUseCase;
    private readonly IGetContactActivityUseCase _getContactActivityUseCase;
    private readonly IGetCampaignPreviewUseCase _getCampaignPreviewUseCase;
    private readonly ISendCampaignEmailUseCase _sendCampaignEmailUseCase;
    private readonly IUploadCampaignImageUseCase _uploadCampaignImageUseCase;
    private readonly IQueueCampaignUseCase _queueCampaignUseCase;
    private readonly IGetCampaignStatusUseCase _getCampaignStatusUseCase;
    private readonly IGetActiveCampaignUseCase _getActiveCampaignUseCase;
    private readonly ICancelCampaignUseCase _cancelCampaignUseCase;

    public ContactsController(
        IListContactsUseCase listContactsUseCase,
        ISearchContactsUseCase searchContactsUseCase,
        IUpdateContactUseCase updateContactUseCase,
        ISoftDeleteContactUseCase softDeleteContactUseCase,
        IHardDeleteContactUseCase hardDeleteContactUseCase,
        IListContactInsightsUseCase listContactInsightsUseCase,
        IGetContactActivityUseCase getContactActivityUseCase,
        IGetCampaignPreviewUseCase getCampaignPreviewUseCase,
        ISendCampaignEmailUseCase sendCampaignEmailUseCase,
        IUploadCampaignImageUseCase uploadCampaignImageUseCase,
        IQueueCampaignUseCase queueCampaignUseCase,
        IGetCampaignStatusUseCase getCampaignStatusUseCase,
        IGetActiveCampaignUseCase getActiveCampaignUseCase,
        ICancelCampaignUseCase cancelCampaignUseCase)
    {
        _listContactsUseCase = listContactsUseCase;
        _searchContactsUseCase = searchContactsUseCase;
        _updateContactUseCase = updateContactUseCase;
        _softDeleteContactUseCase = softDeleteContactUseCase;
        _hardDeleteContactUseCase = hardDeleteContactUseCase;
        _listContactInsightsUseCase = listContactInsightsUseCase;
        _getContactActivityUseCase = getContactActivityUseCase;
        _getCampaignPreviewUseCase = getCampaignPreviewUseCase;
        _sendCampaignEmailUseCase = sendCampaignEmailUseCase;
        _uploadCampaignImageUseCase = uploadCampaignImageUseCase;
        _queueCampaignUseCase = queueCampaignUseCase;
        _getCampaignStatusUseCase = getCampaignStatusUseCase;
        _getActiveCampaignUseCase = getActiveCampaignUseCase;
        _cancelCampaignUseCase = cancelCampaignUseCase;
    }

    [HttpGet("{businessId:guid}/insights")]
    public async Task<IActionResult> Insights(
        Guid businessId,
        [FromQuery] int inactiveDays = 60,
        [FromQuery] bool? inactiveOnly = null,
        [FromQuery] bool? hasEmailOnly = null,
        [FromQuery] string? search = null)
    {
        var result = await _listContactInsightsUseCase.Execute(
            businessId,
            inactiveDays,
            inactiveOnly,
            hasEmailOnly,
            search);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/{contactId:guid}/activity")]
    public async Task<IActionResult> Activity(Guid businessId, Guid contactId, [FromQuery] int take = 15)
    {
        var result = await _getContactActivityUseCase.Execute(businessId, contactId, take);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/campaign/preview")]
    public async Task<IActionResult> CampaignPreview(
        Guid businessId,
        [FromQuery] int inactiveDays = 60,
        [FromQuery] int quietDays = 60,
        [FromQuery] string? audienceMode = null)
    {
        var mode = CampaignAudienceModeParser.Parse(audienceMode);
        var result = await _getCampaignPreviewUseCase.Execute(businessId, inactiveDays, quietDays, mode);
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/campaign/send")]
    public async Task<IActionResult> CampaignSend(Guid businessId, [FromBody] SendCampaignEmailRequestDto request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        try
        {
            var result = await _sendCampaignEmailUseCase.Execute(businessId, request);
            if (result.Status == "Failed")
                return UnprocessableEntity(result);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{businessId:guid}/campaign/queue")]
    public async Task<IActionResult> CampaignQueue(Guid businessId, [FromBody] QueueCampaignRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _queueCampaignUseCase.Execute(businessId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{businessId:guid}/campaign/active")]
    public async Task<IActionResult> CampaignActive(Guid businessId)
    {
        var result = await _getActiveCampaignUseCase.Execute(businessId);
        if (result == null)
            return NoContent();
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/campaign/cancel")]
    public async Task<IActionResult> CampaignCancel(Guid businessId, [FromQuery] Guid? campaignId = null)
    {
        var result = await _cancelCampaignUseCase.Execute(businessId, campaignId);
        if (result == null)
            return NoContent();
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/campaign/{campaignId:guid}/status")]
    public async Task<IActionResult> CampaignStatus(Guid businessId, Guid campaignId)
    {
        var result = await _getCampaignStatusUseCase.Execute(businessId, campaignId);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/campaign/upload-image")]
    [RequestSizeLimit(CampaignImageLimits.MaxUploadBytes)]
    public async Task<IActionResult> CampaignUploadImage(
        Guid businessId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Image file is required." });

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(90));

            await using var stream = file.OpenReadStream();
            var result = await _uploadCampaignImageUseCase.Execute(
                businessId,
                stream,
                file.Length,
                file.FileName,
                file.ContentType,
                timeoutCts.Token);
            return Ok(result);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(504, new { message = "La subida tardó demasiado. Probá otra imagen o más tarde." });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
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
