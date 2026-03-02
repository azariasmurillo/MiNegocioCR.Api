using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.Business
{
    public interface IGetBusinessByIdUseCase
    {        
        Task<GetBusinessByIdResultDto?> Execute(Guid id);        
    }
}
