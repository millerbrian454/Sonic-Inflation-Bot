using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using SonicInflatorService.Core;

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
