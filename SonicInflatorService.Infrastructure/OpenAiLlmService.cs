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
        private readonly List<string> _models;
        private List<string>.Enumerator _model;

        public OpenAiLlmService(IConfiguration config)
        {
            
            _apiKey = config["OpenAI:ApiKey"]!;
            _baseUri = config["OpenAI:BaseUri"]!;
            _models = config.GetSection("OpenAI:Models").Get<List<string>>();
            _model = _models.GetEnumerator();
            _model.MoveNext();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUri)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }


        public async Task<string> GenerateResponseAsync(string prompt)
        {
            int retryCount = _models.Count;

            while (retryCount-- > 0)
            {
                try
                {
                    var request = new
                    {
                        model = _model.Current,
                        messages = new[]
                        {
                            new { role = "system", content = "You are mimicking a Discord user." },
                            new { role = "user", content = prompt }
                        },
                        temperature = 0.7,
                        max_tokens = 512
                    };

                    StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync("chat/completions", content);
                    string result = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(result))
                    {
                        JsonDocument json = JsonDocument.Parse(result);

                        if (json.RootElement.TryGetProperty("error", out JsonElement error))
                        {
                            string? errorMessage = error.GetProperty("message").GetString();
                            if (errorMessage?.Contains("rate limit", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                Log.Warning($"Rate limit hit for model {_model.Current}. Switching to next model.");
                                MoveToNextModel();
                                continue;
                            }

                            Log.Error($"OpenAI API returned an error: {errorMessage}");
                            return string.Empty;
                        }

                        // Parse and return normal response
                        return json.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
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

            Log.Error("All models exhausted or failed due to rate limits.");
            return string.Empty;
        }
        private void MoveToNextModel()
        {
            if (!_model.MoveNext())
            {
                _model = _models.GetEnumerator();
                _model.MoveNext();
            }
        }
    }
}
