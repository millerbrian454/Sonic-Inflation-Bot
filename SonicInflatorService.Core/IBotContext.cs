using Discord.WebSocket;

namespace SonicInflatorService.Core
{
    public interface IBotContext
    {
        DiscordSocketClient Client { get; }
        DiscordSettings Settings { get; }
    }
}
