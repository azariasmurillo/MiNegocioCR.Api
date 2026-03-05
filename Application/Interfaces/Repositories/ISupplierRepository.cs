using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task AddSupplierAsync(Supplier supplier);

        Task<List<Supplier>> GetSuppliersAsync(Guid businessId);

        Task<Supplier?> GetSupplierAsync(Guid id, Guid businessId);
    }
}
