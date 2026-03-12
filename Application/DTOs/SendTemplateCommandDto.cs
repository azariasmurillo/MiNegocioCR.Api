namespace MiNegocioCR.Api.Application.DTOs
{
    public class SendTemplateCommandDto
    {
        public Guid BusinessId { get; set; }
        public string Phone { get; set; } = default!;
        public Guid TemplateId { get; set; }
    }
}
