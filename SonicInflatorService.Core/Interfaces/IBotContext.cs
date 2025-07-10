using Discord.WebSocket;

namespace SonicInflatorService.Core.Interfaces
{
    public interface IBotContext
    {
        DiscordSocketClient Client { get; }
        DiscordSettings Settings { get; }
    }
}
