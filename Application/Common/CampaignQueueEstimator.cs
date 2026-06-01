namespace MiNegocioCR.Api.Application.Common;

public sealed class CampaignScheduleEstimate
{
    public int QueuePosition { get; init; }
    public int PendingBeforeCampaign { get; init; }
    public DateTime EstimatedStartUtc { get; init; }
    public DateTime EstimatedEndUtc { get; init; }
    public int EstimatedCalendarDays { get; init; }
}

public static class CampaignQueueEstimator
{
    public static CampaignScheduleEstimate Estimate(
        DateTime utcNow,
        int pendingBeforeCampaign,
        int recipientsInCampaign,
        int sentTodayGlobal,
        int dailyLimit,
        int intervalSeconds)
    {
        if (recipientsInCampaign <= 0)
        {
            return new CampaignScheduleEstimate
            {
                QueuePosition = pendingBeforeCampaign + 1,
                PendingBeforeCampaign = pendingBeforeCampaign,
                EstimatedStartUtc = utcNow,
                EstimatedEndUtc = utcNow,
                EstimatedCalendarDays = 0
            };
        }

        var remainingToday = Math.Max(0, dailyLimit - sentTodayGlobal);
        var firstIndex = pendingBeforeCampaign;
        var lastIndex = pendingBeforeCampaign + recipientsInCampaign - 1;

        var startUtc = ResolveSlotUtc(utcNow, firstIndex, remainingToday, dailyLimit, intervalSeconds);
        var endUtc = ResolveSlotUtc(utcNow, lastIndex, remainingToday, dailyLimit, intervalSeconds);
        var calendarDays = Math.Max(1, (int)Math.Ceiling((endUtc.Date - utcNow.Date).TotalDays) + 1);

        return new CampaignScheduleEstimate
        {
            QueuePosition = pendingBeforeCampaign + 1,
            PendingBeforeCampaign = pendingBeforeCampaign,
            EstimatedStartUtc = startUtc,
            EstimatedEndUtc = endUtc,
            EstimatedCalendarDays = calendarDays
        };
    }

    internal static DateTime ResolveSlotUtc(
        DateTime utcNow,
        int zeroBasedIndex,
        int remainingToday,
        int dailyLimit,
        int intervalSeconds)
    {
        if (zeroBasedIndex < 0)
            return utcNow;

        var slot = 0;
        var dayOffset = 0;
        var remaining = zeroBasedIndex + 1;

        while (remaining > 0)
        {
            var cap = dayOffset == 0 ? remainingToday : dailyLimit;
            if (cap <= 0)
            {
                dayOffset++;
                continue;
            }

            if (remaining <= cap)
            {
                slot = remaining - 1;
                break;
            }

            remaining -= cap;
            dayOffset++;
        }

        var minutesFromNow = dayOffset * 24 * 60 + slot * (intervalSeconds / 60.0);
        return utcNow.AddMinutes(minutesFromNow);
    }
}
