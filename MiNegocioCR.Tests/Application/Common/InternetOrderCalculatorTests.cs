using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class InternetOrderCalculatorTests
{
    [Fact]
    public void ComputeTotals_MultiLineUsdAndAdvances_MatchesExpected()
    {
        var lines = new List<InternetOrderLineInputDto>
        {
            new() { ProductName = "A", ProductUrl = "https://a.com", UnitPriceUsd = 25.99m, Quantity = 1 },
            new() { ProductName = "B", ProductUrl = "https://b.com", UnitPriceUsd = 10m, Quantity = 2 }
        };
        var advances = new List<InternetOrderAdvanceInputDto>
        {
            new() { AmountCrc = 40000m },
            new() { AmountCrc = 9914.80m }
        };

        var result = InternetOrderCalculator.ComputeTotals(
            exchangeRateApplied: 520m,
            internationalShippingCost: 15000m,
            localCourierCost: 3000m,
            serviceFee: 8000m,
            lines,
            advances);

        result.LinesTotalUsd.Should().Be(45.99m);
        result.LinesTotalCrc.Should().Be(23914.80m);
        result.SubtotalCrc.Should().Be(49914.80m);
        result.TotalAdvancesCrc.Should().Be(49914.80m);
        result.BalanceDueCrc.Should().Be(0m);
    }

    [Fact]
    public void ComputeTotals_WhenAdvancesExceedTotal_Throws()
    {
        var lines = new List<InternetOrderLineInputDto>
        {
            new() { ProductName = "A", ProductUrl = "https://a.com", UnitPriceUsd = 1m, Quantity = 1 }
        };

        var act = () => InternetOrderCalculator.ComputeTotals(500m, 0, 0, 0, lines,
            new List<InternetOrderAdvanceInputDto> { new() { AmountCrc = 999999m } });

        act.Should().Throw<ArgumentException>();
    }
}
