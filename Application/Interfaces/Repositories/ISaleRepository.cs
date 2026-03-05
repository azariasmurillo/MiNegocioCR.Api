using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ISaleRepository
    {
        Task AddSaleAsync(Sale sale);

        Task<List<Sale>> GetSalesAsync(Guid businessId);

        Task<Sale?> GetSaleAsync(Guid id, Guid businessId);
    }
    
}
