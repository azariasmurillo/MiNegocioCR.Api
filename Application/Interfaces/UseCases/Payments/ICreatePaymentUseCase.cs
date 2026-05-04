using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;

public interface ICreatePaymentUseCase
{
    Task<object> Execute(CreatePaymentRequestDto request);
}
