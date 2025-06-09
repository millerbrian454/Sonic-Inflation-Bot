namespace SonicInflatorService.Services
{
    public interface IDiscordChannelService
    {
        ulong SelectRandomChannelId(DiscordSettings settings, Random randomGenerator);
    }
    public class DiscordChannelService : IDiscordChannelService
    {
        private readonly ILogger<DiscordChannelService> _logger;

        public DiscordChannelService(ILogger<DiscordChannelService> logger)
        {
            _logger = logger;
        }

        public ulong SelectRandomChannelId(DiscordSettings settings, Random randomGenerator)
        {
            ulong selectedChannelId;
            int randomChannelChancePercentage = settings.RandomChannelPercentageChance;
            bool randomChannelChance = randomGenerator.Next(100) < randomChannelChancePercentage;

            if (randomChannelChance)
            {
                selectedChannelId = settings.ChannelIds[randomGenerator.Next(settings.ChannelIds.Count)];
                _logger.LogInformation($"[{randomChannelChancePercentage}% chance] Randomly selected channel.");
            }
            else
            {
                selectedChannelId = settings.PrimaryChannelId;
            }

            return selectedChannelId;
        }



    }
}
