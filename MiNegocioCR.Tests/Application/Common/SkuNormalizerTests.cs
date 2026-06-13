using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Domain.Entities;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class SkuNormalizerTests
{
    [Theory]
    [InlineData("  LAPTOP-HP840G5  ", "LAPTOP-HP840G5", "laptop-hp840g5")]
    [InlineData("hp", "hp", "hp")]
    public void NormalizeSku_and_key(string raw, string expectedSku, string expectedKey)
    {
        SkuNormalizer.NormalizeSku(raw).Should().Be(expectedSku);
        SkuNormalizer.ToNormalizedKey(raw).Should().Be(expectedKey);
    }

    [Fact]
    public void Apply_sets_business_and_normalized_fields()
    {
        var businessId = Guid.NewGuid();
        var variant = new CatalogVariant { CatalogItemId = Guid.NewGuid() };

        SkuNormalizer.Apply(variant, businessId, " MOUSE-LOGI ");

        variant.BusinessId.Should().Be(businessId);
        variant.SKU.Should().Be("MOUSE-LOGI");
        variant.SkuNormalized.Should().Be("mouse-logi");
    }

    [Fact]
    public void NormalizeSku_empty_returns_null()
    {
        SkuNormalizer.NormalizeSku("   ").Should().BeNull();
        SkuNormalizer.ToNormalizedKey(null).Should().BeNull();
    }
}
