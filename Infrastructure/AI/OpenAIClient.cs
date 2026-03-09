using MiNegocioCR.Api.Application.AI.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiNegocioCR.Api.Infrastructure.AI
{
    public class OpenAIClient : IAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIClient> _logger;

        public OpenAIClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
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
                _logger.LogInformation("OpenAI RAW: {Json}", responseContent);

                var doc = JsonDocument.Parse(responseContent);

                var message = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message");

                string result = "";

                if (message.TryGetProperty("content", out var content))
                {
                    if (content.ValueKind == JsonValueKind.String)
                    {
                        result = content.GetString() ?? "";
                    }
                    else if (content.ValueKind == JsonValueKind.Array)
                    {
                        result = content[0].GetProperty("text").GetString() ?? "";
                    }
                }

                return result;
            }
            catch
            {
                return "Hubo un problema procesando tu solicitud. Intenta nuevamente.";
            }            
        }
    }
}
