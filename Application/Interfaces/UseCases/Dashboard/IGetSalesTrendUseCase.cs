using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetSalesTrendUseCase
{
    Task<List<SalesTrendPointDto>> Execute(Guid businessId, DateTime? from, DateTime? to, string? groupBy);
}
