using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class CostaRicaTimeTests
{
    [Fact]
    public void ToUtcStartOfDay_May27CostaRica_IsSixHoursAfterUtcMidnight()
    {
        var day = new DateOnly(2026, 5, 27);
        var utc = CostaRicaTime.ToUtcStartOfDay(day);
        utc.Should().Be(new DateTime(2026, 5, 27, 6, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ToUtcEndExclusive_IsStartOfNextLocalDay()
    {
        var day = new DateOnly(2026, 5, 27);
        CostaRicaTime.ToUtcEndExclusive(day)
            .Should().Be(CostaRicaTime.ToUtcStartOfDay(day.AddDays(1)));
    }

    [Fact]
    public void ToLocalDate_ConvertsUtcToCostaRicaCalendarDate()
    {
        var utc = new DateTime(2026, 5, 27, 5, 30, 0, DateTimeKind.Utc);
        CostaRicaTime.ToLocalDate(utc).Should().Be(new DateOnly(2026, 5, 26));
    }
}
