namespace MiNegocioCR.Api.Application.AI.Models
{
    public class ToolResult
    {
        public string Message { get; set; } = "";

        public Guid? ProductId { get; set; }

        public string ProductName { get; set; } = "";

        public decimal? Price { get; set; }
    }
}
