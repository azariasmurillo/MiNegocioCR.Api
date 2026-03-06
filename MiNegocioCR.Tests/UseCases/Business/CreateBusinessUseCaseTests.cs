using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Business;

public class CreateBusinessUseCaseTests
{
    private static IAppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CreatesBusinessAndReturnsIdAndName()
    {
        await using var context = (AppDbContext)CreateInMemoryContext();
        var sut = new CreateBusinessUseCase(context);

        var request = new CreateBusinessRequestDto { Name = "Mi Tienda" };

        var result = await sut.Execute(request);

        result.Should().NotBeNull();

        var business = await context.Businesses.FirstOrDefaultAsync(b => b.Name == "Mi Tienda");
        business.Should().NotBeNull();
        business!.Name.Should().Be("Mi Tienda");
        var settings = await context.BusinessSettings.FindAsync(business.Id);
        settings.Should().NotBeNull();
        settings!.NextOrderNumber.Should().Be(1);
    }

    [Fact]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await using var context = (AppDbContext)CreateInMemoryContext();
        var sut = new CreateBusinessUseCase(context);

        var act = () => sut.Execute(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }
}
