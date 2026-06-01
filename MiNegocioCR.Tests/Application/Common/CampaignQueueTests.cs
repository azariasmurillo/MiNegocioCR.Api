using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class CampaignPersonalizationTests
{
    [Fact]
    public void ApplySubjectTemplate_PrependsFirstName_WhenNoPlaceholder()
    {
        var result = CampaignPersonalization.ApplySubjectTemplate("Feliz Navidad", "Juan Pérez");
        result.Should().Be("Juan, Feliz Navidad");
    }

    [Fact]
    public void ApplySubjectTemplate_ReplacesPlaceholder()
    {
        var result = CampaignPersonalization.ApplySubjectTemplate("{nombre}, feliz Navidad", "María López");
        result.Should().Be("María, feliz Navidad");
    }
}

public class CampaignQueueEstimatorTests
{
    [Fact]
    public void Estimate_SpreadsAcrossDays_WhenExceedsDailyLimit()
    {
        var utcNow = new DateTime(2026, 5, 29, 10, 0, 0, DateTimeKind.Utc);
        var estimate = CampaignQueueEstimator.Estimate(
            utcNow,
            pendingBeforeCampaign: 0,
            recipientsInCampaign: 300,
            sentTodayGlobal: 0,
            dailyLimit: 495,
            intervalSeconds: 60);

        estimate.EstimatedStartUtc.Should().Be(utcNow);
        estimate.EstimatedEndUtc.Should().BeAfter(utcNow);
        estimate.EstimatedCalendarDays.Should().BeGreaterThan(0);
    }
}
