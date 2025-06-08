namespace SonicInflatorService
{
    public class DiscordSettings
    {
        public required string Token { get; set; }
        public ulong PrimaryChannelId { get; set; }
        public required List<ulong> ChannelIds { get; set; }
        public string? ImagePath { get; set; }
        public int RandomIntervalMinutesMaxValue { get; set; }
        public int RandomIntervalMinutesMinValue { get; set; }
        public int RandomChannelPercentageChance { get; set; }
        public int ResponseCooldownIntervalMinutes { get; set; }
    }
}
