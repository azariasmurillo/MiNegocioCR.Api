using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Infrastructure.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MiNegocioCR.Tests.Infrastructure.Services;

public class LocalImageSharpProductImageEnhancerServiceTests
{
    [Fact]
    public async Task EnhanceAsync_returns_three_webp_streams()
    {
        await using var input = new MemoryStream();
        using (var image = new Image<Rgba32>(40, 20, new Rgba32(200, 50, 50, 255)))
        {
            await image.SaveAsPngAsync(input);
        }

        input.Position = 0;
        var sut = new LocalImageSharpProductImageEnhancerService();

        var result = await sut.EnhanceAsync(
            input,
            new ProductImageEnhanceOptions { MarketplaceStyle = MarketplaceStylePresets.WhiteV1 },
            CancellationToken.None);

        try
        {
            Assert.True(result.Main.Length > 0);
            Assert.True(result.Mobile.Length > 0);
            Assert.True(result.Thumbnail.Length > 0);
        }
        finally
        {
            await result.Main.DisposeAsync();
            await result.Mobile.DisposeAsync();
            await result.Thumbnail.DisposeAsync();
        }
    }

    [Fact]
    public async Task EnhanceAsync_throws_when_background_removal_requested()
    {
        await using var input = new MemoryStream();
        using (var image = new Image<Rgba32>(10, 10, Color.White))
        {
            await image.SaveAsPngAsync(input);
        }

        input.Position = 0;
        var sut = new LocalImageSharpProductImageEnhancerService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.EnhanceAsync(
                input,
                new ProductImageEnhanceOptions { UseBackgroundRemoval = true },
                CancellationToken.None));
    }
}
