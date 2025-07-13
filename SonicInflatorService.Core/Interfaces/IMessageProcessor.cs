using Discord.WebSocket;

namespace SonicInflatorService.Core.Interfaces
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(SocketMessage message);
    }
}
