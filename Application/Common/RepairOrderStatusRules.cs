using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Transiciones: Pending→InProcess, InProcess→Processed, Processed→Delivered;
/// cancelación solo desde Pending o InProcess. Sin saltos; terminal Delivered o Cancelled.
/// </summary>
public static class RepairOrderStatusRules
{
    public static bool IsValidTransition(RepairOrderStatus from, RepairOrderStatus to)
    {
        if (from == to)
            return true;

        return (from, to) switch
        {
            (RepairOrderStatus.Pending, RepairOrderStatus.InProcess) => true,
            (RepairOrderStatus.Pending, RepairOrderStatus.Cancelled) => true,
            (RepairOrderStatus.InProcess, RepairOrderStatus.Processed) => true,
            (RepairOrderStatus.InProcess, RepairOrderStatus.Cancelled) => true,
            (RepairOrderStatus.Processed, RepairOrderStatus.Delivered) => true,
            _ => false
        };
    }
}
