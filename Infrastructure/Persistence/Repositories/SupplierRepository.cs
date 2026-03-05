using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddSupplierAsync(Supplier supplier)
        {
            if (supplier == null)
                throw new ArgumentNullException(nameof(supplier));
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Supplier>> GetSuppliersAsync(Guid businessId)
        {
            return await _context.Suppliers
                .Where(x => x.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<Supplier?> GetSupplierAsync(Guid id, Guid businessId)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(x => x.Id == id && x.BusinessId == businessId);
        }
    }
}
