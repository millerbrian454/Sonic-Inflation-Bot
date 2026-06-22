using System.Text.Json.Serialization;

namespace SonicInflatorService.Infrastructure.Contracts;

public class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public OllamaMessage[] Messages { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    // Ignore this if it's null so Ollama doesn't reject it
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("format")]
    public string? Format { get; set; } 

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keep_alive")]
    public int? KeepAlive { get; set; } 
}