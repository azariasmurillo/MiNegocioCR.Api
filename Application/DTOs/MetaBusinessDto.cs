using System.Text.Json.Serialization;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class MetaBusinessDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
