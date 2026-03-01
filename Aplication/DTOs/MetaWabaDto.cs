using System.Text.Json.Serialization;

namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class MetaWabaDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
