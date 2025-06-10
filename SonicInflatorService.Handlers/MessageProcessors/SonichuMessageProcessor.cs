using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class SonichuMessageProcessor : MessageProcessorBase
    {
        private const string PATTERN = @"\b(sanic|sonichu)\b";
        public SonichuMessageProcessor(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context, PATTERN)
        {
        }

        public override Task ProcessAsync(SocketMessage message)
        {
            return message.Channel.SendFileAsync(Context.Settings.SonichuImagePath, $"OH NO. {message.Author.Mention} HAS SUMMONED SANIC!");
        }
    }
}
