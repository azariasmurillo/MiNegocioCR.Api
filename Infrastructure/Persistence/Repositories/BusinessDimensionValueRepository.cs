using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class BusinessDimensionValueRepository : IBusinessDimensionValueRepository
    {
        private readonly AppDbContext _context;

        public BusinessDimensionValueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BusinessDimensionValue>> GetByBusinessAndDimensionAsync(
            Guid businessId,
            string dimensionName,
            bool includeInactive = false)
        {
            var dimKey = dimensionName.Trim().ToLowerInvariant();
            var query = _context.BusinessDimensionValues
                .AsNoTracking()
                .Where(x => x.BusinessId == businessId && x.DimensionName.ToLower() == dimKey);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query
                .OrderBy(x => x.Value)
                .ToListAsync();
        }

        public async Task<BusinessDimensionValue?> FindByKeyAsync(Guid businessId, string valueKey)
        {
            return await _context.BusinessDimensionValues
                .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.ValueKey == valueKey);
        }

        public async Task UpsertAsync(BusinessDimensionValue entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var existing = await FindByKeyAsync(entry.BusinessId, entry.ValueKey);
            if (existing == null)
            {
                await _context.BusinessDimensionValues.AddAsync(entry);
            }
            else
            {
                existing.Value = entry.Value;
                existing.DimensionName = entry.DimensionName;
                existing.IsActive = true;
                _context.BusinessDimensionValues.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
    }
}
