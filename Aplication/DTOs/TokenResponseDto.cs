using System.Text.Json.Serialization;

namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class TokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
