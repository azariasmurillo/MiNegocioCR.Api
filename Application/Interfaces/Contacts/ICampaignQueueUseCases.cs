using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IQueueCampaignUseCase
{
    Task<QueueCampaignResultDto> Execute(Guid businessId, QueueCampaignRequestDto request);
}

public interface IGetCampaignStatusUseCase
{
    Task<CampaignStatusDto?> Execute(Guid businessId, Guid campaignId);
}

public interface IGetActiveCampaignUseCase
{
    Task<CampaignStatusDto?> Execute(Guid businessId);
}

public interface ICampaignQueueProcessor
{
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default);
}
