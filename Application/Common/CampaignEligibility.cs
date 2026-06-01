using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class CampaignEligibility
{
    public static bool HasValidEmail(Domain.Entities.Contact contact) =>
        !string.IsNullOrWhiteSpace(contact.Email) && contact.Email.Contains('@');

    public static bool IsInactive(Domain.Entities.Contact contact, int inactiveDays, DateTime utcNow)
    {
        if (inactiveDays < 1)
            inactiveDays = CampaignLimits.DefaultInactiveDays;

        var threshold = utcNow.AddDays(-inactiveDays);
        return contact.LastActivityAt == null || contact.LastActivityAt < threshold;
    }

    public static bool IsOutsideQuietPeriod(Domain.Entities.Contact contact, int quietDays, DateTime utcNow)
    {
        if (quietDays < 1)
            quietDays = CampaignLimits.DefaultQuietDays;

        if (contact.LastMarketingEmailAt == null)
            return true;

        var threshold = utcNow.AddDays(-quietDays);
        return contact.LastMarketingEmailAt < threshold;
    }

    public static bool IsEligible(
        Domain.Entities.Contact contact,
        int inactiveDays,
        int quietDays,
        DateTime utcNow,
        CampaignAudienceMode audienceMode = CampaignAudienceMode.Inactive) =>
        !contact.IsDeleted
        && HasValidEmail(contact)
        && (audienceMode == CampaignAudienceMode.AllWithEmail
            || (IsInactive(contact, inactiveDays, utcNow)
                && IsOutsideQuietPeriod(contact, quietDays, utcNow)));
}
