using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

public interface IGetSalesByBusinessUseCase
{
    Task<PagedResultDto<SalesListItemDto>> Execute(Guid businessId, SalesListQueryDto query);
}
