using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Business;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Business;

public class GetBusinessByIdUseCaseTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WhenBusinessExists_ReturnsDto()
    {
        await using var context = CreateInMemoryContext();
        var business = new BusinessEntity
        {
            Id = Guid.NewGuid(),
            Name = "Mi Negocio",
            IsActive = true,
            EnableEmailNotifications = true,
            SmtpHost = "smtp.test.com"
        };
        context.Businesses.Add(business);
        await context.SaveChangesAsync();

        var sut = new GetBusinessByIdUseCase(context);

        var result = await sut.Execute(business.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(business.Id);
        result.Name.Should().Be("Mi Negocio");
        result.IsActive.Should().BeTrue();
        result.SmtpHost.Should().Be("smtp.test.com");
    }

    [Fact]
    public async Task Execute_WhenBusinessDoesNotExist_ReturnsNull()
    {
        await using var context = CreateInMemoryContext();
        var sut = new GetBusinessByIdUseCase(context);

        var result = await sut.Execute(Guid.NewGuid());

        result.Should().BeNull();
    }
}
