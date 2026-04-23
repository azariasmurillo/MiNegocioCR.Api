namespace MiNegocioCR.Api.Domain.Enums;

/// <summary>
/// Valores persistidos en <c>RepairOrder.Status</c> (int). No reordenar: son el contrato con la BD.
/// </summary>
public enum RepairOrderStatus
{
    Pending = 1,
    InProcess = 2,
    Processed = 3,
    Delivered = 4,
    Cancelled = 5
}