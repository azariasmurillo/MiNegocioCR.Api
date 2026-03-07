namespace MiNegocioCR.Api.Domain.Entities
{
    public class AITokenUsage
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public DateTime Date { get; set; }

        public int TokensUsed { get; set; }
    }
}
