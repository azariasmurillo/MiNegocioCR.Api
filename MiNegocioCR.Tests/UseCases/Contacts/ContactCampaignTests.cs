using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Contacts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Moq;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Tests.UseCases.Contacts;

public class ContactCampaignTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    [Fact]
    public async Task Preview_ReturnsEligibleContactsWithEmail()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();

        ctx.Contacts.AddRange(
            new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = "Elegible",
                Phone = "50688880001",
                Email = "elegible@test.com",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-90)
            },
            new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = "Activo",
                Phone = "50688880002",
                Email = "activo@test.com",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-5)
            },
            new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = "Sin email",
                Phone = "50688880003",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-120)
            });
        await ctx.SaveChangesAsync();

        var sut = new GetCampaignPreviewUseCase(ctx);
        var result = await sut.Execute(businessId, inactiveDays: 60, quietDays: 60);

        result.EligibleContacts.Should().HaveCount(1);
        result.EligibleContacts[0].Email.Should().Be("elegible@test.com");
        result.Quota.DailyLimit.Should().Be(CampaignLimits.PlatformDailyLimit);
    }

    [Fact]
    public async Task Preview_AllWithEmail_ReturnsEveryContactWithEmail()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();

        ctx.Contacts.AddRange(
            new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = "Inactivo",
                Phone = "50688880001",
                Email = "inactivo@test.com",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-90)
            },
            new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = "Activo",
                Phone = "50688880002",
                Email = "activo@test.com",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-5)
            },
            new Contact
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = "Sin email",
                Phone = "50688880003",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-120)
            });
        await ctx.SaveChangesAsync();

        var sut = new GetCampaignPreviewUseCase(ctx);
        var result = await sut.Execute(businessId, audienceMode: CampaignAudienceMode.AllWithEmail);

        result.AudienceMode.Should().Be(nameof(CampaignAudienceMode.AllWithEmail));
        result.EligibleContacts.Should().HaveCount(2);
        result.EligibleContacts.Select(c => c.Email).Should().BeEquivalentTo(["inactivo@test.com", "activo@test.com"]);
    }

    [Fact]
    public async Task Send_AllWithEmail_AllowsActiveContact()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity
        {
            Id = businessId,
            Name = "JoyCaTech",
            PublicEmail = "joycatech@gmail.com",
            EnableEmailNotifications = true,
            CreatedAt = DateTime.UtcNow
        });
        ctx.Contacts.Add(new Contact
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Cliente activo",
            Phone = "50688880005",
            Email = "activo@test.com",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow.AddDays(-2)
        });
        await ctx.SaveChangesAsync();

        var email = new Mock<IEmailService>();
        email.Setup(x => x.SendCampaignAsync(
                It.IsAny<BusinessEntity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("msg-456");

        var sut = new SendCampaignEmailUseCase(ctx, email.Object);
        var result = await sut.Execute(businessId, new SendCampaignEmailRequestDto
        {
            ContactId = contactId,
            Subject = "Feliz Navidad",
            BodyText = "Te deseamos lo mejor este mes. Visitá nuestra tienda para ver promociones.",
            AudienceMode = nameof(CampaignAudienceMode.AllWithEmail)
        });

        result.Status.Should().Be("Sent");
    }

    [Fact]
    public async Task Send_UpdatesLastMarketingEmailAt()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity
        {
            Id = businessId,
            Name = "JoyCaTech",
            PublicEmail = "joycatech@gmail.com",
            EnableEmailNotifications = true,
            CreatedAt = DateTime.UtcNow
        });
        ctx.Contacts.Add(new Contact
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Cliente",
            Phone = "50688880004",
            Email = "cliente@test.com",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow.AddDays(-100)
        });
        await ctx.SaveChangesAsync();

        var email = new Mock<IEmailService>();
        email.Setup(x => x.SendCampaignAsync(
                It.IsAny<BusinessEntity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("msg-123");

        var sut = new SendCampaignEmailUseCase(ctx, email.Object);
        var result = await sut.Execute(businessId, new SendCampaignEmailRequestDto
        {
            ContactId = contactId,
            Subject = "Te extrañamos",
            BodyText = "Hace tiempo que no nos visitás.",
            InactiveDays = 60,
            QuietDays = 60
        });

        result.Status.Should().Be("Sent");
        var contact = await ctx.Contacts.AsNoTracking().SingleAsync(c => c.Id == contactId);
        contact.LastMarketingEmailAt.Should().NotBeNull();
        (await ctx.ContactEmailCampaignLogs.CountAsync()).Should().Be(1);
    }

    [Fact]
    public void ValidateContent_RequiresTextOrImage()
    {
        var act = () => CampaignContentValidator.ValidateContent(null, null);
        act.Should().Throw<ArgumentException>();
    }
}
