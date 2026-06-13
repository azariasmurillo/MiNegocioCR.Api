using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IBusinessDimensionValueRepository
    {
        Task<List<BusinessDimensionValue>> GetByBusinessAndDimensionAsync(
            Guid businessId,
            string dimensionName,
            bool includeInactive = false);

        Task<BusinessDimensionValue?> FindByKeyAsync(Guid businessId, string valueKey);

        Task UpsertAsync(BusinessDimensionValue entry);
    }
}
