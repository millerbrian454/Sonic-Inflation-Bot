namespace SonicInflatorService.Core.Entities
{
    public class OpenAIConfigurationEntity : BaseEntity
    {
        public required string ApiKey { get; set; }
        public required string BaseUri { get; set; }
        
        // Navigation property
        public ICollection<OpenAIModelEntity> Models { get; set; } = new List<OpenAIModelEntity>();
    }
}
