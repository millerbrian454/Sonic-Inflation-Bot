namespace SonicInflatorService
{
    public class DiscordSettings
    {
        public required string Token { get; set; }
        public ulong ChannelId { get; set; }
        public string? ImagePath { get; set; }
        public int RandomIntervalMinutesMaxValue { get; set; }
        public int RandomIntervalMinutesMinValue { get; set; }
    }
}
