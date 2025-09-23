namespace SonicInflatorService.Core.Entities
{
    public class DiscordConfigurationEntity
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public ulong PrimaryChannelId { get; set; }
        public ulong GuildId { get; set; }
        public ulong MimicUserId { get; set; }
        public ulong SirenEmojiId { get; set; }
        public string? SirenEmojiName { get; set; }
        public string? InflatedImagePath { get; set; }
        public string? DeflatedImagePath { get; set; }
        public string? SonichuImagePath { get; set; }
        public string? CurseYeHaMeHaImagePath { get; set; }
        public int RandomIntervalMinutesMaxValue { get; set; }
        public int RandomIntervalMinutesMinValue { get; set; }
        public int ResponseCooldownIntervalSeconds { get; set; }
        public int RandomChannelPercentageChance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<DiscordChannelEntity> ChannelIds { get; set; } = new List<DiscordChannelEntity>();
        public ICollection<DiscordContextChannelEntity> ContextChannelIds { get; set; } = new List<DiscordContextChannelEntity>();
        public ICollection<DiscordNaughtyWordEntity> NaughtyWords { get; set; } = new List<DiscordNaughtyWordEntity>();
        public ICollection<DiscordProfessionalWranglerEntity> ProfessionalSonicWranglerUserIds { get; set; } = new List<DiscordProfessionalWranglerEntity>();
    }
}
