using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SonicInflatorService.Core
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(SocketMessage message);
    }
}
