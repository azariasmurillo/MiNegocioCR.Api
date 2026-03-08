namespace MiNegocioCR.Api.Domain.Entities
{
    public class UpsellRule
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public Guid ProductId { get; set; }

        public Guid SuggestedProductId { get; set; }
    }
}
