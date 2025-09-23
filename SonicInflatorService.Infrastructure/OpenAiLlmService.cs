using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Infrastructure
{
    public class OpenAiLlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationService _configService;
        private string _apiKey = string.Empty;
        private string _baseUri = string.Empty;
        private List<string> _models = new();
        private List<string>.Enumerator _model;
        private string _summary = "There is no previous summary yet since this is the start of the conversation.";
        private string _lastAssistantResponse = string.Empty;
        private bool _initialized = false;

        public OpenAiLlmService(IConfigurationService configService)
        {
            _configService = configService;
            _httpClient = new HttpClient();
        }

        private async Task InitializeAsync()
        {
            if (_initialized) return;

            var config = await _configService.GetOpenAIConfigurationAsync();
            if (config == null)
            {
                throw new InvalidOperationException("OpenAI configuration not found in database");
            }

            _apiKey = config.ApiKey;
            _baseUri = config.BaseUri;
            _models = config.Models.Select(m => m.ModelName).ToList();

            if (_models.Count == 0)
            {
                throw new InvalidOperationException("No OpenAI models configured");
            }

            _model = _models.GetEnumerator();
            _model.MoveNext();

            _httpClient.BaseAddress = new Uri(_baseUri);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            _initialized = true;
        }

        public string GetCurrentModel() => _model.Current;

        public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
        {
            await InitializeAsync();

            var messages = new List<object>();

            systemPrompt = $"""
                            A summary of the conversation you have been having so far is: {_summary}

                            ___

                            {systemPrompt}
                            """;

            messages.Add(new { role = "system", content = systemPrompt });

            if (!string.IsNullOrWhiteSpace(_lastAssistantResponse))
            {
                messages.Add(new { role = "assistant", content = _lastAssistantResponse });
            }

            messages.Add(new { role = "user", content = userPrompt });

            string assistantResponse = await GetAssistantResponseAsync(messages);

            if (!string.IsNullOrWhiteSpace(assistantResponse))
            {
                _lastAssistantResponse = assistantResponse;
                _summary = await SummarizeConversationAsync(_summary, userPrompt, assistantResponse);
            }

            return assistantResponse;
        }

        private Task<string> SummarizeConversationAsync(string previousSummary, string userPrompt, string assistantReply)
        {
            string summarizationPrompt = $"Summarize the conversation so far in 1–7 sentences as needed. Capture the main points and tone.";

            List<object> messages = new List<object>
            {
                new { role = "system", content = "You are a summarizer that condenses conversations into concise summaries." },
                new { role = "assistant", content = $"Previous summary:\n{previousSummary}" },
                new { role = "user", content = $"User: {userPrompt}" },
                new { role = "assistant", content = $"Assistant: {assistantReply}" },
                new { role = "user", content = summarizationPrompt }
            };

            return GetAssistantResponseAsync(messages);
        }

        private async Task<string> GetAssistantResponseAsync(List<object> messages)
        {
            int retryCount = _models.Count;

            while (retryCount-- > 0)
            {
                try
                {
                    var request = new
                    {
                        model = _model.Current,
                        messages = messages,
                        temperature = 0.7,
                        max_tokens = 512
                    };

                    StringContent contentPayload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync("chat/completions", contentPayload);
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

                        string assistantResponse = json.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString()!;

                        return assistantResponse;
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
