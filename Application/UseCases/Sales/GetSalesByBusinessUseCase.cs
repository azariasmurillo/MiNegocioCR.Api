using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

public class GetSalesByBusinessUseCase : IGetSalesByBusinessUseCase
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesByBusinessUseCase(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<PagedResultDto<SalesListItemDto>> Execute(Guid businessId, SalesListQueryDto query)
    {
        query.Page = query.Page <= 0 ? 1 : query.Page;
        query.PageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
        return await _saleRepository.GetPagedSalesAsync(businessId, query);
    }
}
