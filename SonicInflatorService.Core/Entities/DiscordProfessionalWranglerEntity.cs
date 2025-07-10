namespace SonicInflatorService.Core.Entities
{
    public class DiscordProfessionalWranglerEntity
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public int DiscordConfigurationId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public DiscordConfigurationEntity DiscordConfiguration { get; set; } = null!;
    }
}
