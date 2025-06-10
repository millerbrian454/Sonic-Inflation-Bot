using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class SonicDeflationMessageProcessor : MessageProcessorBase
    {
        private const string PATTERN = @"\bALAKAGOO! 👉\b";
        public SonicDeflationMessageProcessor(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context, PATTERN)
        {
        }

        public override Task ProcessAsync(SocketMessage message)
        {
            return message.Channel.SendFileAsync(Context.Settings.DeflatedImagePath, $"SONIC NOOOOOOOOO. {message.Author.Mention} WHAT HAVE YOU DONE?!");
        }
    }
}
