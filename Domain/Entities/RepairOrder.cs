namespace MiNegocioCR.Api.Domain.Entities;

public class RepairOrder
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }

    /// <summary>Número de orden formateado (p. ej. 6 dígitos) por negocio; único con <see cref="BusinessId"/>.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    public string? ProblemDescription { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceTypeOther { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? AccessoriesIncluded { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Password { get; set; }
    public bool IsDiagnosticPaid { get; set; } = false;

    /// <summary>Descuento en porcentaje (0–100) para usar al generar la factura desde la orden.</summary>
    public decimal DiscountPercent { get; set; } = 0m;

    public bool PayCash { get; set; } = false;
    public bool PayTransfer { get; set; } = false;
    public bool PaySinpe { get; set; } = false;
    public bool PayCard { get; set; } = false;

    public int Status { get; set; }

    /// <summary>
    /// Permite excluir órdenes canceladas o archivadas de listados sin borrar el registro.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;

    public ICollection<RepairOrderItem> Items { get; set; } = new List<RepairOrderItem>();
}