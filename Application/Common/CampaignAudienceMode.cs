namespace MiNegocioCR.Api.Application.Common;

public enum CampaignAudienceMode
{
    Inactive = 0,
    AllWithEmail = 1
}

public static class CampaignAudienceModeParser
{
    public static CampaignAudienceMode Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CampaignAudienceMode.Inactive;

        return value.Trim().ToLowerInvariant() switch
        {
            "allwithemail" or "all" or "general" => CampaignAudienceMode.AllWithEmail,
            _ => CampaignAudienceMode.Inactive
        };
    }
}
