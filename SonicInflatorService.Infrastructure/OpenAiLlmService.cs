using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;
using SonicInflatorService.Core;

namespace SonicInflatorService.Infrastructure
{
    public class OpenAiLlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUri;
        private readonly string _model;

        public OpenAiLlmService(IConfiguration config)
        {
            
            _apiKey = config["OpenAI:ApiKey"]!;
            _baseUri = config["OpenAI:BaseUri"]!;
            _model = config["OpenAI:Model"]!;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUri)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        
        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are mimicking a Discord user." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 512
                };

                StringContent? content =
                    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                HttpResponseMessage? response = await _httpClient.PostAsync("chat/completions", content);
                string? result = await response.Content.ReadAsStringAsync();
                JsonDocument? json;
                if (!string.IsNullOrEmpty(result))
                {
                    json = JsonDocument.Parse(result);
                    return json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content")
                        .GetString()!;
                }

                Log.Error("Result string from OpenAI API response was null");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception was thrown while trying to generate an AI response message.");
                return string.Empty;
            }
        }
    }
}
