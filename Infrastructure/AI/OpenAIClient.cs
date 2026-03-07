using MiNegocioCR.Api.Application.AI.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiNegocioCR.Api.Infrastructure.AI
{
    public class OpenAIClient : IAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AskAsync(string prompt, string model, int maxTokens)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.4,
                max_tokens = maxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return "El asistente no está disponible en este momento.";
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(responseContent);

                var result = doc
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return result ?? "";
            }
            catch
            {
                return "Hubo un problema procesando tu solicitud. Intenta nuevamente.";
            }            
        }
    }
}
