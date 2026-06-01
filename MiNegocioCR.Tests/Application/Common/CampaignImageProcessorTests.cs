using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class CampaignImageProcessorTests
{
    [Fact]
    public async Task Optimize_ResizesWideImageAndReducesSize()
    {
        await using var source = new MemoryStream();
        using (var image = new Image<Rgba32>(2000, 1200))
        {
            image.SaveAsPng(source);
        }
        source.Position = 0;

        var result = await CampaignImageProcessor.OptimizeAsync(source);
        await using (result.Output)
        {
            result.Width.Should().BeLessThanOrEqualTo(CampaignImageLimits.MaxDisplayWidth);
            result.ContentType.Should().Be("image/jpeg");
            result.OptimizedBytes.Should().BeLessThan(source.Length);
            result.OptimizedBytes.Should().BeLessThanOrEqualTo(CampaignImageLimits.TargetOptimizedBytes);
        }
    }
}
