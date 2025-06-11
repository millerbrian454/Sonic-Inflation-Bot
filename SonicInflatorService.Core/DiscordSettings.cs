namespace SonicInflatorService.Core
{
    public class DiscordSettings
    {
        public required string Token { get; set; }
        public ulong PrimaryChannelId { get; set; }
        public ulong SirenEmojiId { get; set; }
        public ulong GuildId { get; set; }
        public ulong MimicUserId { get; set; }

        public string SirenEmojiName { get; set; }
        public required List<ulong> ChannelIds { get; set; }
        public required List<ulong> ContextChannelIds { get; set; }
        public string? InflatedImagePath { get; set; }
        public string? DeflatedImagePath { get; set; }
        public string? SonichuImagePath { get; set; }
        public string? CurseYeHaMeHaImagePath { get; set; }
        public int RandomIntervalMinutesMaxValue { get; set; }
        public int RandomIntervalMinutesMinValue { get; set; }
        public int RandomChannelPercentageChance { get; set; }
        public int ResponseCooldownIntervalSeconds { get; set; }
        public required List<ulong> ProfessionalSonicWranglerUserIds { get; set; }
    }
}
