using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

public static class InternetOrderStatusRules
{
    public static bool IsValidTransition(InternetOrderStatus from, InternetOrderStatus to)
    {
        if (from == to)
            return true;

        return (from, to) switch
        {
            (InternetOrderStatus.Created, InternetOrderStatus.Purchased) => true,
            (InternetOrderStatus.Created, InternetOrderStatus.Cancelled) => true,
            (InternetOrderStatus.Purchased, InternetOrderStatus.Received) => true,
            (InternetOrderStatus.Purchased, InternetOrderStatus.Cancelled) => true,
            (InternetOrderStatus.Received, InternetOrderStatus.Delivered) => true,
            (InternetOrderStatus.Received, InternetOrderStatus.Cancelled) => true,
            _ => false
        };
    }

    public static bool IsTerminal(InternetOrderStatus status) =>
        status is InternetOrderStatus.Delivered or InternetOrderStatus.Cancelled;
}
