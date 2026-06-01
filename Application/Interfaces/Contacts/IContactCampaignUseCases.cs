using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IGetCampaignPreviewUseCase
{
    Task<CampaignPreviewResultDto> Execute(
        Guid businessId,
        int inactiveDays = 60,
        int quietDays = 60,
        CampaignAudienceMode audienceMode = CampaignAudienceMode.Inactive);
}

public interface ISendCampaignEmailUseCase
{
    Task<SendCampaignEmailResultDto> Execute(Guid businessId, SendCampaignEmailRequestDto request);
}

public interface IUploadCampaignImageUseCase
{
    Task<UploadCampaignImageResultDto> Execute(
        Guid businessId,
        Stream fileStream,
        long fileLength,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default);
}
