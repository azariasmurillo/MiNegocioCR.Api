using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;

namespace MiNegocioCR.Api.Application.UseCases.Business;

public class GetBusinessConfigUseCase : IGetBusinessConfigUseCase
{
    private readonly IAppDbContext _context;

    public GetBusinessConfigUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessConfigDto?> Execute(Guid businessId)
    {
        return await _context.Businesses
            .AsNoTracking()
            .Where(x => x.Id == businessId)
            .Select(x => new BusinessConfigDto
            {
                LogoUrl = x.LogoUrl,
                BusinessType = x.BusinessType,
                Description = x.Description,
                Phone = x.Phone,
                Location = x.Location,
                PublicEmail = x.PublicEmail
            })
            .FirstOrDefaultAsync();
    }
}
