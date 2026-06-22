namespace SonicInflatorService.Core.Entities
{
    public class OpenAIModelEntity : AiModelBaseEntity
    {
        public int OpenAIConfigurationId { get; set; }
        
        // Navigation property
        public OpenAIConfigurationEntity OpenAIConfiguration { get; set; } = null!;
    }
}
