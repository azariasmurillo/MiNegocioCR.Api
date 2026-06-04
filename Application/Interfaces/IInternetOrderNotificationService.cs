using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Application.Interfaces;

public interface IInternetOrderNotificationService
{
    Task NotifyPurchasedAsync(BusinessEntity business, InternetOrder order);
    Task NotifyReceivedAsync(BusinessEntity business, InternetOrder order);
}
