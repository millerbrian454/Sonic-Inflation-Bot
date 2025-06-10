using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class SonicInflationMessageProcessor : MessageProcessorBase
    {
        private const string PATTERN = @"^(?!.*@sonic-inflator).*?\b(sonic|inflat\w*)\b";
        private readonly IChannelTracker _tracker;

        public SonicInflationMessageProcessor(ILoggerFactory loggerFactory, IBotContext context, IChannelTracker tracker) : base(loggerFactory, context, PATTERN)
        {
            _tracker = tracker;
        }

        public override Task ProcessAsync(SocketMessage message)
        {
            _tracker.TrackLastImage(message.Channel.Id, Context.Settings.InflatedImagePath);
            return message.Channel.SendFileAsync(Context.Settings.InflatedImagePath, $"DID {message.Author.Mention} SAY SONIC INFLATION?!");
        }
    }
}
