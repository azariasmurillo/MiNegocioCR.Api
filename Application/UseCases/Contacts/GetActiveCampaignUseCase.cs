using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class GetActiveCampaignUseCase : IGetActiveCampaignUseCase
{
    private static readonly string[] ActiveStatuses = ["Queued", "InProgress"];

    private readonly IAppDbContext _context;

    public GetActiveCampaignUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignStatusDto?> Execute(Guid businessId)
    {
        var campaign = await _context.EmailCampaigns
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && ActiveStatuses.Contains(c.Status))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (campaign == null)
            return null;

        return await GetCampaignStatusUseCase.BuildStatusAsync(_context, campaign);
    }
}
