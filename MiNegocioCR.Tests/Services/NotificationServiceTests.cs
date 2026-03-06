using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Services;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _sut = new NotificationService(_emailServiceMock.Object);
    }

    [Fact]
    public async Task SendOrderCreatedAsync_WhenOrderAndBusinessValidAndEmailEnabled_SendsEmail()
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            EnableEmailNotifications = true
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = 42,
            CustomerEmail = "client@test.com",
            Business = business
        };
        _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.SendOrderCreatedAsync(order);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                business,
                "client@test.com",
                "Orden #42 creada",
                "Su orden fue creada correctamente."),
            Times.Once);
    }

    [Fact]
    public async Task SendOrderCreatedAsync_WhenOrderIsNull_DoesNotCallEmailService()
    {
        await _sut.SendOrderCreatedAsync(null!);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendOrderCreatedAsync_WhenBusinessIsNull_DoesNotCallEmailService()
    {
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = 1,
            CustomerEmail = "a@b.com",
            Business = null!
        };

        await _sut.SendOrderCreatedAsync(order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendOrderCreatedAsync_WhenEmailNotificationsDisabled_DoesNotCallEmailService()
    {
        var business = new Business { EnableEmailNotifications = false };
        var order = new RepairOrder
        {
            OrderNumber = 1,
            CustomerEmail = "a@b.com",
            Business = business
        };

        await _sut.SendOrderCreatedAsync(order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendOrderCreatedAsync_WhenCustomerEmailIsEmpty_DoesNotCallEmailService()
    {
        var business = new Business { EnableEmailNotifications = true };
        var order = new RepairOrder
        {
            OrderNumber = 1,
            CustomerEmail = "",
            Business = business
        };

        await _sut.SendOrderCreatedAsync(order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendOrderProcessedAsync_CompletesWithoutThrowing()
    {
        var order = new RepairOrder { OrderNumber = 1 };

        var act = () => _sut.SendOrderProcessedAsync(order);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendOrderDeliveredAsync_CompletesWithoutThrowing()
    {
        var order = new RepairOrder { OrderNumber = 1 };

        var act = () => _sut.SendOrderDeliveredAsync(order);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendOrderCancelledAsync_CompletesWithoutThrowing()
    {
        var order = new RepairOrder { OrderNumber = 1 };

        var act = () => _sut.SendOrderCancelledAsync(order);

        await act.Should().NotThrowAsync();
    }
}
