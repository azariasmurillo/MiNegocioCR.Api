using System.Threading;
using FluentAssertions;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Application.UseCases.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Domain.Exceptions;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Whatsapp;

public class WhatsappApplicationServiceTests
{
    private readonly Mock<IWhatsappService> _whatsappServiceMock;
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<IWhatsappMessageRepository> _messageRepositoryMock;
    private readonly Mock<IGetBusinessByIdUseCase> _getBusinessByIdMock;
    private readonly Mock<IWhatsAppTokenService> _whatsAppTokenServiceMock;
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly WhatsappApplicationService _sut;

    public WhatsappApplicationServiceTests()
    {
        _whatsappServiceMock = new Mock<IWhatsappService>();
        _contextMock = new Mock<IAppDbContext>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _messageRepositoryMock = new Mock<IWhatsappMessageRepository>();
        _getBusinessByIdMock = new Mock<IGetBusinessByIdUseCase>();
        _whatsAppTokenServiceMock = new Mock<IWhatsAppTokenService>();
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _sut = new WhatsappApplicationService(
            _whatsappServiceMock.Object,
            _contextMock.Object,
            _encryptionServiceMock.Object,
            _messageRepositoryMock.Object,
            _getBusinessByIdMock.Object,
            _whatsAppTokenServiceMock.Object,
            _businessRepositoryMock.Object,
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<WhatsappApplicationService>());
    }

    [Fact]
    public async Task SendAsync_WhenBusinessExistsAndWhatsappEnabled_SendsAndSavesMessage()
    {
        var businessId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var conv = new WhatsAppConversation
        {
            Id = convId,
            BusinessId = businessId,
            PhoneNumber = "123"
        };
        var businessDto = new GetBusinessByIdResultDto
        {
            Id = businessId,
            Name = "Test",
            EnableWhatsappNotifications = true,
            WhatsappPhoneNumberId = "phone-id"
        };
        _getBusinessByIdMock.Setup(x => x.Execute(businessId)).ReturnsAsync(businessDto);
        _messageRepositoryMock.Setup(x => x.GetOrCreateConversationAsync(businessId, "123", null))
            .ReturnsAsync(conv);
        _messageRepositoryMock.Setup(x => x.GetConversationByIdAsync(convId, businessId))
            .ReturnsAsync(conv);
        _whatsappServiceMock.Setup(x => x.SendAsync(It.IsAny<GetBusinessByIdResultDto>(), "123", "Hello", null, null))
            .Returns(Task.CompletedTask);
        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<WhatsAppMessage>()))
            .Returns(Task.CompletedTask);
        _messageRepositoryMock.Setup(x => x.UpdateConversationAfterMessageAsync(convId, "Hello", MessageDirection.Outbound))
            .Returns(Task.CompletedTask);

        await _sut.SendAsync(businessId, "123", "Hello");

        _whatsappServiceMock.Verify(
            x => x.SendAsync(businessDto, "123", "Hello", null, null),
            Times.Once);
        _messageRepositoryMock.Verify(
            x => x.SaveAsync(It.Is<WhatsAppMessage>(m =>
                m.Body == "Hello" && m.PhoneNumber == "123" && m.ConversationId == convId)),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenBusinessNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var conv = new WhatsAppConversation { Id = convId, BusinessId = businessId, PhoneNumber = "123" };
        _messageRepositoryMock.Setup(x => x.GetOrCreateConversationAsync(businessId, "123", null))
            .ReturnsAsync(conv);
        _messageRepositoryMock.Setup(x => x.GetConversationByIdAsync(convId, businessId))
            .ReturnsAsync(conv);
        _getBusinessByIdMock.Setup(x => x.Execute(businessId)).ReturnsAsync((GetBusinessByIdResultDto?)null);

        var act = () => _sut.SendAsync(businessId, "123", "Hi");

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "Business");
    }

    [Fact]
    public async Task SendAsync_WhenWhatsappNotEnabled_ThrowsException()
    {
        var businessId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var conv = new WhatsAppConversation { Id = convId, BusinessId = businessId, PhoneNumber = "123" };
        _messageRepositoryMock.Setup(x => x.GetOrCreateConversationAsync(businessId, "123", null))
            .ReturnsAsync(conv);
        _messageRepositoryMock.Setup(x => x.GetConversationByIdAsync(convId, businessId))
            .ReturnsAsync(conv);
        var businessDto = new GetBusinessByIdResultDto
        {
            Id = businessId,
            EnableWhatsappNotifications = false
        };
        _getBusinessByIdMock.Setup(x => x.Execute(businessId)).ReturnsAsync(businessDto);

        var act = () => _sut.SendAsync(businessId, "123", "Hi");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Whatsapp not enabled*");
    }

    [Fact]
    public async Task ConnectAsync_WhenBusinessExistsAndValidCredentials_UpdatesBusinessAndSaves()
    {
        var businessId = Guid.NewGuid();
        var entity = new BusinessEntity { Id = businessId, Name = "Test" };
        _businessRepositoryMock.Setup(x => x.GetByIdAsync(businessId)).ReturnsAsync(entity);
        _whatsappServiceMock.Setup(x => x.ValidateAsync("phone-id", "token")).ReturnsAsync(true);
        _encryptionServiceMock.Setup(x => x.Encrypt("token")).Returns("encrypted");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ConnectAsync(businessId, "phone-id", "token");

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WhenBusinessNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        _businessRepositoryMock.Setup(x => x.GetByIdAsync(businessId)).ReturnsAsync((BusinessEntity?)null);

        var act = () => _sut.ConnectAsync(businessId, "phone-id", "token");

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "Business");
    }

    [Fact]
    public async Task ConnectAsync_WhenValidationFails_ThrowsException()
    {
        var businessId = Guid.NewGuid();
        var entity = new BusinessEntity { Id = businessId };
        _businessRepositoryMock.Setup(x => x.GetByIdAsync(businessId)).ReturnsAsync(entity);
        _whatsappServiceMock.Setup(x => x.ValidateAsync("phone-id", "token")).ReturnsAsync(false);

        var act = () => _sut.ConnectAsync(businessId, "phone-id", "token");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid WhatsApp credentials*");
    }
}
