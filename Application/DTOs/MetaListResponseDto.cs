using System.Text.Json.Serialization;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class MetaListResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
