namespace SonicInflatorService.Core.Entities
{
    public class OpenAIConfigurationEntity
    {
        public int Id { get; set; }
        public required string ApiKey { get; set; }
        public required string BaseUri { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        public ICollection<OpenAIModelEntity> Models { get; set; } = new List<OpenAIModelEntity>();
    }
}
