using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface IGetBusinessByIdUseCase
    {        
        Task<GetBusinessByIdResultDto?> Execute(Guid id);        
    }
}
