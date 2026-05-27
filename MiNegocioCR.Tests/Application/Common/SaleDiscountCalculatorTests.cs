using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Domain.Enums;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class SaleDiscountCalculatorTests
{
    [Theory]
    [InlineData(11_300, 0, 13, 1_300, 11_300)]
    [InlineData(20_000, 0, 13, 2_300.88, 20_000)]
    [InlineData(100_000, 10_000, 13, 10_353.98, 90_000)]
    [InlineData(50_000, 5_000, 13, 5_176.99, 45_000)]
    public void ComputeTotals_TaxInclusive_ExtractsTaxWithoutAdding(
        decimal subtotal,
        decimal discount,
        decimal rate,
        decimal expectedTax,
        decimal expectedTotalOrden)
    {
        var (tax, totalOrden) = SaleDiscountCalculator.ComputeTotals(subtotal, discount, rate);

        tax.Should().Be(expectedTax);
        totalOrden.Should().Be(expectedTotalOrden);
    }

    [Fact]
    public void ComputeTotals_ZeroRate_NoTax()
    {
        var (tax, totalOrden) = SaleDiscountCalculator.ComputeTotals(10_000m, 2_000m, 0m);

        tax.Should().Be(0m);
        totalOrden.Should().Be(8_000m);
    }

    [Fact]
    public void Resolve_PercentDiscount_OnGrossSubtotal()
    {
        var (kind, input, amount) = SaleDiscountCalculator.Resolve(100_000m, "Percent", 10m, 0m);

        kind.Should().Be(SaleDiscountKind.Percent);
        input.Should().Be(10m);
        amount.Should().Be(10_000m);
    }
}
