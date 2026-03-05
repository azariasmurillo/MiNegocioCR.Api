namespace MiNegocioCR.Api.Application.DTOs
{
    public class ConnectWhatsappRequestDto
    {
        public Guid BusinessId { get; set; }
        public string PhoneNumberId { get; set; }
        public string AccessToken { get; set; }
    }
}
