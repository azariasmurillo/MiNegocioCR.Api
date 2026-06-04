using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;
using MiNegocioCR.Api.Application.UseCases.InternetOrders;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Services;
using Moq;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Tests.UseCases.InternetOrders;

public class InternetOrderUseCaseTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private static (
        ICreateInternetOrderUseCase Create,
        IListInternetOrdersByBusinessUseCase List,
        IUpdateInternetOrderStatusUseCase UpdateStatus
    ) CreateSut(AppDbContext ctx)
    {
        IGetInternetOrderByIdUseCase getById = new GetInternetOrderByIdUseCase(ctx);
        return (
            new CreateInternetOrderUseCase(ctx, getById),
            new ListInternetOrdersByBusinessUseCase(ctx),
            new UpdateInternetOrderStatusUseCase(ctx, new InternetOrderNotificationService(new Mock<IEmailService>().Object))
        );
    }

    private static async Task<Guid> SeedBusinessAsync(AppDbContext ctx)
    {
        var businessId = Guid.NewGuid();
        ctx.Businesses.Add(new BusinessEntity
        {
            Id = businessId,
            Name = "Pedidos Internet Test",
            TaxRatePercent = 13m,
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
        return businessId;
    }

    private static UpsertInternetOrderRequestDto SampleRequest() => new()
    {
        CustomerName = "Cliente Prueba",
        CustomerPhone = "88887777",
        CustomerEmail = "cliente@test.com",
        ExchangeRateApplied = 520m,
        InternationalShippingCost = 15000m,
        LocalCourierCost = 3000m,
        ServiceFee = 5000m,
        Lines =
        [
            new InternetOrderLineInputDto
            {
                ProductName = "Echo Dot",
                ProductUrl = "https://amazon.com/echo",
                UnitPriceUsd = 29.99m,
                Quantity = 1
            }
        ],
        Advances = [new InternetOrderAdvanceInputDto { AmountCrc = 10000m }]
    };

    [Fact]
    public async Task Create_List_And_UpdateStatus_Flow_Works()
    {
        await using var ctx = CreateContext();
        var businessId = await SeedBusinessAsync(ctx);
        var (create, list, updateStatus) = CreateSut(ctx);

        var createdRaw = await create.Execute(businessId, SampleRequest());
        createdRaw.Should().NotBeNull();

        var listed = await list.Execute(businessId, null, null);
        listed.Should().NotBeNull();

        var orderId = ExtractId(createdRaw);
        orderId.Should().NotBe(Guid.Empty);

        var statusRaw = await updateStatus.Execute(businessId, orderId, new UpdateInternetOrderStatusRequestDto
        {
            NewStatus = InternetOrderStatus.Purchased
        });
        statusRaw.Should().NotBeNull();
        ExtractStatus(statusRaw).Should().Be("Purchased");
    }

    private static Guid ExtractId(object payload)
    {
        var prop = payload.GetType().GetProperty("Id");
        prop.Should().NotBeNull();
        return (Guid)prop!.GetValue(payload)!;
    }

    private static string ExtractStatus(object payload)
    {
        var prop = payload.GetType().GetProperty("Status");
        prop.Should().NotBeNull();
        return prop!.GetValue(payload)!.ToString()!;
    }
}
