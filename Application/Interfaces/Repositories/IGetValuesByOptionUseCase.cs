using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetValuesByOptionUseCase
    {
        Task<IReadOnlyList<CatalogOptionValueDto>> ExecuteAsync(Guid optionId);
    }
}
