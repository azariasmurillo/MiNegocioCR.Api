namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class SendWhatsappRequestDto
    {
        public Guid BusinessId { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }
}
