using Discord.WebSocket;
using SonicInflatorService.Core;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Infrastructure
{
    public class BotContext : IBotContext
    {
        public DiscordSocketClient Client { get; }
        public DiscordSettings Settings { get; }

        public BotContext(DiscordSocketClient client, Func<DiscordSettings> settingsFactory)
        {
            Client = client;
            Settings = settingsFactory();
        }
    }
}
