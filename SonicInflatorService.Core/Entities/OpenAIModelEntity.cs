namespace SonicInflatorService.Core.Entities
{
    public class OpenAIModelEntity
    {
        public int Id { get; set; }
        public required string ModelName { get; set; }
        public int OpenAIConfigurationId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public OpenAIConfigurationEntity OpenAIConfiguration { get; set; } = null!;
    }
}
