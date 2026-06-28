using Newtonsoft.Json;

namespace SonicInflatorService.Core.Entities;

public class OllamaConfigurationEntity : BaseEntity
{
    [JsonProperty("UseOllama")]
    public bool ShouldUseLlm { get; set; }
    
    [JsonProperty("OllamaBaseUri")]
    public required string BaseUri { get; set; }

    [JsonProperty("OllamaModels")]
    public ICollection<OllamaModelEntity> Models { get; set; } = new List<OllamaModelEntity>();


}