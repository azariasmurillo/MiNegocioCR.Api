using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/credit-accounts")]
public class CreditAccountsController : ControllerBase
{
    private readonly IListCreditAccountsByBusinessUseCase _list;
    private readonly IGetCreditAccountByIdUseCase _getById;
    private readonly IAddCreditChargeUseCase _addCharge;
    private readonly IRegisterCreditPaymentUseCase _registerPayment;
    private readonly IUpdateCreditCommitmentUseCase _updateCommitment;
    private readonly ISendCreditAccountEmailUseCase _sendEmail;
    private readonly IAddCreditCommunicationUseCase _addCommunication;
    private readonly ICancelCreditAccountUseCase _cancelAccount;

    public CreditAccountsController(
        IListCreditAccountsByBusinessUseCase list,
        IGetCreditAccountByIdUseCase getById,
        IAddCreditChargeUseCase addCharge,
        IRegisterCreditPaymentUseCase registerPayment,
        IUpdateCreditCommitmentUseCase updateCommitment,
        ISendCreditAccountEmailUseCase sendEmail,
        IAddCreditCommunicationUseCase addCommunication,
        ICancelCreditAccountUseCase cancelAccount)
    {
        _list = list;
        _getById = getById;
        _addCharge = addCharge;
        _registerPayment = registerPayment;
        _updateCommitment = updateCommitment;
        _sendEmail = sendEmail;
        _addCommunication = addCommunication;
        _cancelAccount = cancelAccount;
    }

    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> List(
        Guid businessId,
        [FromQuery] string? filter = null,
        [FromQuery] string? search = null)
    {
        var result = await _list.Execute(businessId, filter, search);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetById(Guid businessId, Guid id)
    {
        var result = await _getById.Execute(businessId, id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{businessId:guid}/charges")]
    public async Task<IActionResult> CreateCharge(Guid businessId, [FromBody] CreateCreditChargeRequestDto request)
    {
        var result = await _addCharge.Execute(businessId, null, request);
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/{id:guid}/charges")]
    public async Task<IActionResult> AddCharge(Guid businessId, Guid id, [FromBody] CreateCreditChargeRequestDto request)
    {
        var result = await _addCharge.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/{id:guid}/payments")]
    public async Task<IActionResult> RegisterPayment(
        Guid businessId,
        Guid id,
        [FromBody] RegisterCreditPaymentRequestDto request)
    {
        var result = await _registerPayment.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPatch("{businessId:guid}/{id:guid}/commitment")]
    public async Task<IActionResult> UpdateCommitment(
        Guid businessId,
        Guid id,
        [FromBody] UpdateCreditCommitmentRequestDto request)
    {
        var result = await _updateCommitment.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/{id:guid}/send-email")]
    public async Task<IActionResult> SendEmail(
        Guid businessId,
        Guid id,
        [FromBody] SendCreditEmailRequestDto request)
    {
        await _sendEmail.Execute(businessId, id, request);
        return NoContent();
    }

    [HttpPost("{businessId:guid}/{id:guid}/communications")]
    public async Task<IActionResult> AddCommunication(
        Guid businessId,
        Guid id,
        [FromBody] AddCreditCommunicationRequestDto request)
    {
        var result = await _addCommunication.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/{id:guid}/cancel")]
    public async Task<IActionResult> CancelAccount(Guid businessId, Guid id)
    {
        var result = await _cancelAccount.Execute(businessId, id);
        return Ok(result);
    }
}
