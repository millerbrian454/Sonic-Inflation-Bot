using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class SonichuMessageProcessor : MessageProcessorBase
    {
        private const string PATTERN = @"\b(sanic|sonichu)\b";
        private readonly IChannelTracker _tracker;

        public SonichuMessageProcessor(ILoggerFactory loggerFactory, IBotContext context, IChannelTracker tracker) : base(loggerFactory, context, PATTERN)
        {
            _tracker = tracker;
        }

        public override Task ProcessAsync(SocketMessage message)
        {
            _tracker.TrackLastImage(message.Channel.Id, Context.Settings.SonichuImagePath);
            return message.Channel.SendFileAsync(Context.Settings.SonichuImagePath, $"OH NO. {message.Author.Mention} HAS SUMMONED SANIC!");
        }
    }
}
