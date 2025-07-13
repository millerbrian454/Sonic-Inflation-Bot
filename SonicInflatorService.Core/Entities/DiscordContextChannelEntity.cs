namespace SonicInflatorService.Core.Entities
{
    public class DiscordContextChannelEntity
    {
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public int DiscordConfigurationId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public DiscordConfigurationEntity DiscordConfiguration { get; set; } = null!;
    }
}
