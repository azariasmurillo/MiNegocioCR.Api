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
            requestedPrice: 999m);
        p.Should().Be(999m);
    }

    [Fact]
    public void Resolve_WhenCostAndMargin_AppliesFormula()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 100m,
            profitMargin: 20m,
            requestedPrice: 0m);
        p.Should().Be(120m);
    }

    [Fact]
    public void Resolve_WhenMarginNull_UsesRequestedPrice()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 100m,
            profitMargin: null,
            requestedPrice: 55.5m);
        p.Should().Be(55.5m);
    }

    [Fact]
    public void Resolve_WhenCostZero_UsesRequestedPrice()
    {
        var p = CatalogVariantPriceResolver.ResolvePersistedPrice(
            setPriceManually: false,
            costPrice: 0m,
            profitMargin: 30m,
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
            requestedPrice: 0m);
        p.Should().Be(13.33m);
    }
}
