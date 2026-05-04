using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ISaleRepository
    {
        Task AddSaleAsync(Sale sale);

        Task<List<Sale>> GetSalesAsync(Guid businessId);

        Task<Sale?> GetSaleAsync(Guid id, Guid businessId);

        /// <summary>Lectura por id (incluye ítems y contacto). Multi-tenant opcional vía capa superior.</summary>
        Task<Sale?> GetSaleByIdAsync(Guid id);

        Task<PagedResultDto<SalesListItemDto>> GetPagedSalesAsync(Guid businessId, SalesListQueryDto query);
    }
    
}
