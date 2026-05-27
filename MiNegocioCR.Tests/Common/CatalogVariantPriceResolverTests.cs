using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using Xunit;

namespace MiNegocioCR.Tests.Common;

public class CatalogVariantPriceResolverTests
{
    [Fact]
    public void Resolve_WhenManual_UsesRequestedPrice()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: true,
            costPrice: 100m,
            profitMargin: 25m,
            taxRatePercent: 13m,
            requestedPrice: 999m);
        p.Should().Be(1000m);
    }

    [Fact]
    public void Resolve_WhenCostAndMargin_AppliesFormula_WithoutTax()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 100m,
            profitMargin: 20m,
            taxRatePercent: 0m,
            requestedPrice: 0m);
        p.Should().Be(120m);
    }

    [Fact]
    public void Resolve_WhenCostMarginAndTax_AppliesStepwiseFormula()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 12_000m,
            profitMargin: 35m,
            taxRatePercent: 13m,
            requestedPrice: 0m);
        // Ganancia 4200 → neto 16200 → IVA 2106 → bruto 18306 → múltiplo 5 = 18310
        p.Should().Be(18_310m);
    }

    [Fact]
    public void Resolve_WhenMarginNull_UsesRequestedPrice()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 100m,
            profitMargin: null,
            taxRatePercent: 13m,
            requestedPrice: 55.5m);
        p.Should().Be(60m);
    }

    [Fact]
    public void Resolve_WhenCostZero_UsesRequestedPrice()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 0m,
            profitMargin: 30m,
            taxRatePercent: 13m,
            requestedPrice: 40m);
        p.Should().Be(40m);
    }

    [Fact]
    public void Resolve_RoundsToTwoDecimals()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 10m,
            profitMargin: 33.333m,
            taxRatePercent: 0m,
            requestedPrice: 0m);
        p.Should().Be(15m);
    }
}
