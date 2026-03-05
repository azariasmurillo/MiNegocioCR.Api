using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface ICreateBusinessUseCase
    {
        Task<object> Execute(CreateBusinessRequestDto request);
    }
}
