using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.CreditAccounts;

public interface IListCreditAccountsByBusinessUseCase
{
    Task<object> Execute(Guid businessId, string? filter, string? search);
}

public interface IGetCreditAccountByIdUseCase
{
    Task<object?> Execute(Guid businessId, Guid accountId);
}

public interface IAddCreditChargeUseCase
{
    Task<object> Execute(Guid businessId, Guid? accountId, CreateCreditChargeRequestDto request);
}

public interface IRegisterCreditPaymentUseCase
{
    Task<object> Execute(Guid businessId, Guid accountId, RegisterCreditPaymentRequestDto request);
}

public interface IUpdateCreditCommitmentUseCase
{
    Task<object> Execute(Guid businessId, Guid accountId, UpdateCreditCommitmentRequestDto request);
}

public interface ISendCreditAccountEmailUseCase
{
    Task Execute(Guid businessId, Guid accountId, SendCreditEmailRequestDto request);
}

public interface IAddCreditCommunicationUseCase
{
    Task<object> Execute(Guid businessId, Guid accountId, AddCreditCommunicationRequestDto request);
}

public interface IGetCreditDashboardSummaryUseCase
{
    Task<object> Execute(Guid businessId);
}

public interface ICancelCreditAccountUseCase
{
    Task<object> Execute(Guid businessId, Guid accountId);
}
