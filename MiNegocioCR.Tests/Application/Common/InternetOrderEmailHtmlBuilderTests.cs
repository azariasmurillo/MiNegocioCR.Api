using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Tests.Application.Common;

public class InternetOrderEmailHtmlBuilderTests
{
    [Fact]
    public void Build_IncludesLogoLinesTotalsAndViewButton()
    {
        var business = new BusinessEntity
        {
            Id = Guid.NewGuid(),
            Name = "Joyca Tech",
            LogoUrl = "https://cdn.example.com/logo.png",
            Phone = "88887777",
        };
        var order = new InternetOrder
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            OrderNumber = "20260604001",
            Status = (int)InternetOrderStatus.Purchased,
            LinesTotalUsd = 100m,
            LinesTotalCrc = 52000m,
            InternationalShippingCost = 5000m,
            LocalCourierCost = 2000m,
            ServiceFee = 1000m,
            SubtotalCrc = 60000m,
            TotalAdvancesCrc = 10000m,
            BalanceDueCrc = 50000m,
            Contact = new Contact { Name = "Ismael Blanco", Phone = "8888", Email = "test@test.com" },
            Lines =
            [
                new InternetOrderLine
                {
                    ProductName = "Echo Dot",
                    ProductUrl = "https://amazon.com/echo",
                    UnitPriceUsd = 100m,
                    Quantity = 1,
                    LineTotalUsd = 100m,
                    LineTotalCrc = 52000m,
                }
            ],
            Advances =
            [
                new InternetOrderAdvance { AmountCrc = 10000m, PaidAt = DateTime.UtcNow, Method = "SINPE" }
            ],
        };

        var html = InternetOrderEmailHtmlBuilder.Build(
            business,
            order,
            "Pedido comprado",
            "Tu pedido fue comprado.");

        html.Should().Contain("logo.png");
        html.Should().Contain("Echo Dot");
        html.Should().Contain("Subtotal productos (USD)");
        html.Should().NotContain("Ver detalle del pedido");
        html.Should().NotContain("ExchangeRate");
    }
}
