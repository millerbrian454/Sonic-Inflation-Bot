using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class SonicInflationMessageProcessor : MessageProcessorBase
    {
        private const string PATTERN = @"\b(sonic|inflat\w*)\b";
        public SonicInflationMessageProcessor(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context, PATTERN)
        {
        }

        public override Task ProcessAsync(SocketMessage message)
        {
            return message.Channel.SendFileAsync(Context.Settings.InflatedImagePath, $"DID {message.Author.Mention} SAY SONIC INFLATION?!");
        }
    }
}
