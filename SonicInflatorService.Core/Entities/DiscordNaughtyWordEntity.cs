namespace SonicInflatorService.Core.Entities
{
    public class DiscordNaughtyWordEntity
    {
        public int Id { get; set; }
        public int DiscordConfigurationId { get; set; }
        public string NaughtyWord { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public DiscordConfigurationEntity DiscordConfiguration { get; set; } = null!;
    }
}
