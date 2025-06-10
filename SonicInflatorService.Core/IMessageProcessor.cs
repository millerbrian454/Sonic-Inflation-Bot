using Discord.WebSocket;

namespace SonicInflatorService.Core
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(SocketMessage message);
    }
}
