using System.Text.Json.Serialization;

namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class MetaListResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
