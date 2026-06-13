using MiNegocioCR.Api.Application.Common;

namespace MiNegocioCR.Tests.Application.Common;

public class SkuImageFileNameParserTests
{
    [Theory]
    [InlineData("LAPTOP-HP840G5_1.jpg", "LAPTOP-HP840G5", 1)]
    [InlineData("mouse.logi_2.jpeg", "mouse.logi", 2)]
    [InlineData("SKU_3.png", "SKU", 3)]
    [InlineData("A_1.webp", "A", 1)]
    [InlineData("folder/sub/M185_1.JPG", "M185", 1)]
    public void TryParse_valid_names(string path, string sku, int slot)
    {
        var ok = SkuImageFileNameParser.TryParse(path, out var result);

        Assert.True(ok);
        Assert.Equal(sku, result.Sku);
        Assert.Equal(slot, result.SortOrder);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-slot.jpg")]
    [InlineData("SKU_4.jpg")]
    [InlineData("SKU_0.jpg")]
    [InlineData("SKU_1.gif")]
    [InlineData("_1.jpg")]
    public void TryParse_invalid_names(string path)
    {
        Assert.False(SkuImageFileNameParser.TryParse(path, out _));
    }
}
