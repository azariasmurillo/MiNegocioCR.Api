namespace MiNegocioCR.Api.Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        /// <summary>Última vez que el contacto generó un pago (venta o abono de reparación).</summary>
        public DateTime? LastActivityAt { get; set; }
        /// <summary>Última vez que se le envió un correo de campaña de marketing.</summary>
        public DateTime? LastMarketingEmailAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
        public ICollection<InternetOrder> InternetOrders { get; set; } = new List<InternetOrder>();
        public ICollection<CreditAccount> CreditAccounts { get; set; } = new List<CreditAccount>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
