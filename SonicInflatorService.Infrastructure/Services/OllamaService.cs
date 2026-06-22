using SonicInflatorService.Core.Interfaces;
using SonicInflatorService.Infrastructure.Contracts;
using System.Text;
using System.Text.Json;
using Serilog;
using SonicInflatorService.Core.Entities;
using SonicInflatorService.Infrastructure.Extensions;
namespace SonicInflatorService.Infrastructure.Services
{
    public class OllamaService : ILlmService
    {
        
        private readonly IConfigurationService _configService;
        private readonly HttpClient _httpClient;
        private string _baseUri = string.Empty;
        private List<string> _models = new();
        private List<string>.Enumerator _model;
        private bool _initialized = false;

        public OllamaService(IConfigurationService configService)
        {
            _httpClient = new HttpClient();
            _configService = configService;
        }

        private async Task InitializeAsync()
        {
            if (_initialized) return;

            OllamaConfigurationEntity? config = await _configService.GetOllamaConfigurationAsync();

            config.CheckConfigForNull();
            
            _baseUri = config.BaseUri;
            _models = config.Models.Select(m => m.ModelName).ToList();

            _models.CheckModelCount();

            _model = _models.GetEnumerator();
            _model.MoveNext();
            
            string safeUri = _baseUri.EndsWith("/") ? _baseUri : _baseUri + "/";
            _httpClient.BaseAddress = new Uri(safeUri);
            
            _initialized = true;
        }

        public string GetCurrentModel() => _model.Current;

        public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
        {
            await InitializeAsync();
            
            var payload = new OllamaChatRequest()
            {
                Model = GetCurrentModel(),
                Stream = false,
                KeepAlive = -1, // Keep the model in memory to prevent the empty "load" response
                Messages = new[]
                {
                    new OllamaMessage { Role = "system", Content = systemPrompt },
                    new OllamaMessage { Role = "user", Content = userPrompt }
                },
                Options = new OllamaOptions
                {
                    NumCtx = 16384, // Inflates model context boundary to 16k tokens
                    Temperature = 0.9f
                }
            };
            
            string jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("/api/chat", content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(jsonResponse);

                if (chatResponse?.Message?.Content == null)
                {
                    Log.Warning("Local model did not return a valid chat structure."); 
                }

                string reply = chatResponse.Message.Content;

                
                // Discord message character limit safety check
                return reply != null && reply.Length > 2000 ? reply.Substring(0, 1997) + "..." : reply ?? string.Empty;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception was thrown while trying to generate an AI response message using ollama.");
                return string.Empty;
            }
        }
    } 
}


