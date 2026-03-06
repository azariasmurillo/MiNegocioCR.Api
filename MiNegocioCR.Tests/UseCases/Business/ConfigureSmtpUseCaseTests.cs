using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Domain.Exceptions;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Business;

public class ConfigureSmtpUseCaseTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WithValidDto_UpdatesBusinessSmtpSettings()
    {
        await using var context = CreateInMemoryContext();
        var business = new BusinessEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            SmtpHost = null,
            SmtpPort = null
        };
        context.Businesses.Add(business);
        await context.SaveChangesAsync();

        var dto = new ConfigureSmtpDto
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "user",
            SmtpPassword = "pass",
            SmtpFromEmail = "from@example.com",
            SmtpFromName = "From Name"
        };
        var sut = new ConfigureSmtpUseCase(context);

        await sut.Execute(business.Id, dto);

        await context.Entry(business).ReloadAsync();
        business.SmtpHost.Should().Be("smtp.example.com");
        business.SmtpPort.Should().Be(587);
        business.SmtpUsername.Should().Be("user");
        business.SmtpFromEmail.Should().Be("from@example.com");
        business.SmtpFromName.Should().Be("From Name");
    }

    [Fact]
    public async Task Execute_WhenDtoIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new ConfigureSmtpUseCase(context);

        var act = () => sut.Execute(Guid.NewGuid(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("dto");
    }

    [Fact]
    public async Task Execute_WhenBusinessNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new ConfigureSmtpUseCase(context);
        var dto = new ConfigureSmtpDto
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "u",
            SmtpPassword = "p",
            SmtpFromEmail = "e@e.com",
            SmtpFromName = "N"
        };

        var act = () => sut.Execute(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "Business");
    }
}
