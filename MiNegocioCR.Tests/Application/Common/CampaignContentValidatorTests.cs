using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class CampaignContentValidatorTests
{
    [Theory]
    [InlineData("{nombre}")]
    [InlineData("Juan")]
    [InlineData("  {nombre}  ")]
    public void ValidateSubject_RejectsTooShort(string subject)
    {
        var act = () => CampaignContentValidator.ValidateSubject(subject);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*8*");
    }

    [Theory]
    [InlineData("{nombre}, promoción navideña")]
    [InlineData("Feliz Navidad 2026")]
    public void ValidateSubject_AcceptsMeaningfulSubject(string subject)
    {
        var act = () => CampaignContentValidator.ValidateSubject(subject);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateContent_RequiresTextOrImage()
    {
        var act = () => CampaignContentValidator.ValidateContent(null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateContent_RejectsThinBodyWithoutImage()
    {
        var act = () => CampaignContentValidator.ValidateContent("Hola {nombre}", null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*25*");
    }

    [Fact]
    public void ValidateContent_AcceptsBodyWithEnoughText()
    {
        var body = "Hola {nombre}, tenemos promoción especial hasta fin de mes. Visitá la tienda.";
        var act = () => CampaignContentValidator.ValidateContent(body, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateContent_AllowsShorterBodyWhenImagePresent()
    {
        var act = () => CampaignContentValidator.ValidateContent("Promo navidad", "https://cdn.example/img.jpg");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Hola {nombre},")]
    public void GetSubstantiveText_StripsPlaceholder(string input)
    {
        CampaignContentValidator.GetSubstantiveText(input).Should().Be("Hola");
    }

    [Fact]
    public void GetSubstantiveText_EmptyWhenOnlyPlaceholder()
    {
        CampaignContentValidator.GetSubstantiveText("{nombre}").Should().BeEmpty();
    }
}
