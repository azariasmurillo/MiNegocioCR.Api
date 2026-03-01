using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.Business
{
    public interface ICreateBusinessUseCase
    {
        Task<object> Execute(CreateBusinessRequestDto request);
    }
}
