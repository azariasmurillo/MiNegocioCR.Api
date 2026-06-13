namespace MiNegocioCR.Api.Domain.Entities
{
    /// <summary>
    /// Valor reutilizable de una dimensión a nivel tenant (ej. Marca=Logitech, Color=Negro).
    /// </summary>
    public class BusinessDimensionValue
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        /// <summary>Nombre canónico de la dimensión (estándar o personalizada).</summary>
        public string DimensionName { get; set; } = string.Empty;

        /// <summary>Valor mostrado normalizado.</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>Clave de comparación (dimensión + valor, case-insensitive).</summary>
        public string ValueKey { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Business Business { get; set; } = null!;
    }
}
