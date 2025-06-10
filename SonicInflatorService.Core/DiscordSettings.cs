namespace SonicInflatorService.Core
{
    public class DiscordSettings
    {
        public required string Token { get; set; }
        public ulong PrimaryChannelId { get; set; }
        public ulong SirenEmojiId { get; set; }
        public string SirenEmojiName { get; set; }
        public required List<ulong> ChannelIds { get; set; }
        public string? InflatedImagePath { get; set; }
        public string? DeflatedImagePath { get; set; }
        public int RandomIntervalMinutesMaxValue { get; set; }
        public int RandomIntervalMinutesMinValue { get; set; }
        public int RandomChannelPercentageChance { get; set; }
        public int ResponseCooldownIntervalSeconds { get; set; }
    }
}
