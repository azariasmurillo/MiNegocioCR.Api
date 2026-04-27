using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Business;

public class UpdateBusinessConfigUseCase : IUpdateBusinessConfigUseCase
{
    private readonly IAppDbContext _context;

    public UpdateBusinessConfigUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessConfigDto> Execute(Guid businessId, UpdateBusinessConfigRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var business = await _context.Businesses
            .FirstOrDefaultAsync(x => x.Id == businessId);

        if (business == null)
            throw new NotFoundException("Business", "Business not found");

        business.BusinessType = Normalize(request.BusinessType);
        business.Description = Normalize(request.Description);
        business.Phone = Normalize(request.Phone);
        business.Location = Normalize(request.Location);
        business.PublicEmail = Normalize(request.PublicEmail);

        await _context.SaveChangesAsync(CancellationToken.None);

        return new BusinessConfigDto
        {
            LogoUrl = business.LogoUrl,
            BusinessType = business.BusinessType,
            Description = business.Description,
            Phone = business.Phone,
            Location = business.Location,
            PublicEmail = business.PublicEmail
        };
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}
