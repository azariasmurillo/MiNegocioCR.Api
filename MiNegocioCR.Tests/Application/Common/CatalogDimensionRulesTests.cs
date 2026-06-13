using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class CatalogDimensionRulesTests
{
    [Theory]
    [InlineData("marca", "Marca")]
    [InlineData("COLOR", "Color")]
    [InlineData("presentación", "Presentación")]
    public void TryResolveStandardName_MatchesCanonical(string input, string expected)
    {
        CatalogDimensionRules.TryResolveStandardName(input, out var canonical).Should().BeTrue();
        canonical.Should().Be(expected);
    }

    [Theory]
    [InlineData("blanco", "Color", "Blanco")]
    [InlineData("LOGITECH", "Marca", "Logitech")]
    [InlineData("HP", "Marca", "HP")]
    [InlineData("xs", "Talla", "XS")]
    [InlineData("128gb", "Capacidad", "128 GB")]
    [InlineData("usb-c", "Compatibilidad", "USB-C")]
    public void NormalizeDimensionValue_AppliesDimensionRules(string raw, string dimension, string expected)
    {
        CatalogDimensionRules.NormalizeDimensionValue(raw, dimension).Should().Be(expected);
    }

    [Fact]
    public void ValidateAndNormalizeDimensionName_AllowsCustomWhenFlagged()
    {
        var name = CatalogDimensionRules.ValidateAndNormalizeDimensionName("Quilates", isCustomDimension: true);
        name.Should().Be("Quilates");
    }

    [Fact]
    public void ValidateAndNormalizeDimensionName_RejectsStandardAsCustom()
    {
        var act = () => CatalogDimensionRules.ValidateAndNormalizeDimensionName("Color", isCustomDimension: true);
        act.Should().Throw<ArgumentException>();
    }
}
