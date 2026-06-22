using System.Text.Json.Serialization;

namespace SonicInflatorService.Infrastructure.Contracts;

public class OllamaMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } // "system", "user", or "assistant"

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("images")]
    public string[]? Images { get; set; } 
}