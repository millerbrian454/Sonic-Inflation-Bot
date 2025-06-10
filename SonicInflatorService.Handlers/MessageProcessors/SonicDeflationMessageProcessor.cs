using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class SonicDeflationMessageProcessor : MessageProcessorBase
    {
        private const string PATTERN = @"ALAKAGOO! 👉";
        private readonly IChannelTracker _tracker;

        public SonicDeflationMessageProcessor(ILoggerFactory loggerFactory, IBotContext context, IChannelTracker tracker) : base(loggerFactory, context, PATTERN)
        {
            _tracker = tracker;
        }

        public override Task ProcessAsync(SocketMessage message)
        {
            
            string lastImage = _tracker.GetLastImage(message.Channel.Id);

            if (lastImage == Context.Settings.SonichuImagePath)
            {
                _tracker.TrackLastImage(message.Channel.Id, Context.Settings.CurseYeHaMeHaImagePath);
                return message.Channel.SendFileAsync(Context.Settings.CurseYeHaMeHaImagePath, $"{message.Author.Mention} JUST TRIED TO DEFLATE SONICHU! CURSE-YE-HA-ME-HAAAAAAAAAAAAA!");
            }
            else
            {
                _tracker.TrackLastImage(message.Channel.Id, Context.Settings.DeflatedImagePath);
                return message.Channel.SendFileAsync(Context.Settings.DeflatedImagePath, $"SONIC NOOOOOOOOO. {message.Author.Mention} WHAT HAVE YOU DONE?!");
            }
        }
    }
}
