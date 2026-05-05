using FluentAssertions;
using MiNegocioCR.Api.Domain.Pricing;
using Xunit;

namespace MiNegocioCR.Tests.Domain.Pricing;

public class CrcSalePriceNormalizerTests
{
    [Fact]
    public void AcceptedExamples_FromPrompt()
    {
        CrcSalePriceNormalizer.NormalizeSalePriceColones(12585m).Should().Be(12585m);
        CrcSalePriceNormalizer.NormalizeSalePriceColones(12586m).Should().Be(12590m);
        CrcSalePriceNormalizer.NormalizeSalePriceColones(12589.6m).Should().Be(12590m);
    }

    [Fact]
    public void Acceptance_FormulaSample()
    {
        // cost × (1 + 25%) redondeado a 2 decimales antes del ceil × 5
        var raw = decimal.Round(10071.68m * 1.25m, 2, MidpointRounding.AwayFromZero);
        raw.Should().Be(12589.60m);
        CrcSalePriceNormalizer.NormalizeSalePriceColones(raw).Should().Be(12590m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public void NonPositive_ReturnsZero(decimal amount)
    {
        CrcSalePriceNormalizer.NormalizeSalePriceColones(amount).Should().Be(0m);
    }

    [Fact]
    public void AlreadyMultipleOf5_UnchangedAfterMoneyRound()
    {
        CrcSalePriceNormalizer.NormalizeSalePriceColones(12585.000001m).Should().Be(12585m);
    }

    [Fact]
    public void JustBelowMultiple_RoundsUp()
    {
        CrcSalePriceNormalizer.NormalizeSalePriceColones(12584.01m).Should().Be(12585m);
    }

    [Fact]
    public void ManyDecimalPlaces_HalfAwayFromZeroThenCeilStep()
    {
        var result = CrcSalePriceNormalizer.NormalizeSalePriceColones(12.5866666666666m);
        result.Should().Be(15m);
    }
}
