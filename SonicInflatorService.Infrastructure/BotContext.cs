using Discord.WebSocket;
using Microsoft.Extensions.Options;
using SonicInflatorService.Core;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Infrastructure
{
    public class BotContext : IBotContext
    {
        public DiscordSocketClient Client { get; }
        public DiscordSettings Settings { get; }

        public BotContext(DiscordSocketClient client, IOptionsMonitor<DiscordSettings> settings)
        {
            Client = client;
            Settings = settings.CurrentValue;
        }
    }
}
