using System.Text.Json.Serialization;

namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class MetaPhoneNumberDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; }
    }
}
