
using Discord.WebSocket;

namespace SonicInflatorService.Core
{
    public interface IEventBinding
    {
        Task RegisterAsync();
    }
}
