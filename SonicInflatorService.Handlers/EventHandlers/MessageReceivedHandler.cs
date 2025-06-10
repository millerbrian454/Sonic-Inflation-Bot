using System.Text.RegularExpressions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.EventHandlers
{
    public class MessageReceivedHandler : EventHandlerBase<SocketMessage>
    {
        private readonly TimeSpan _cooldown;
        private readonly IList<IMessageProcessor> _processors;
        private DateTime _lastResponse;

        public MessageReceivedHandler(ILoggerFactory loggerFactory, IBotContext context, IList<IMessageProcessor> processors) : base(loggerFactory, context)
        {
            _processors = processors;
        }

        public override async Task HandleAsync(SocketMessage message)
        {
            bool isWhitelistedUser = Context.Settings.ProfessionalSonicWranglerUserIds.Contains(message.Author.Id);

            if ((isWhitelistedUser || 
                _lastResponse.Add(_cooldown) <= DateTime.Now)
                && message.Author.Id != Context.Client.CurrentUser.Id
                && !message.Author.IsBot
                && Context.Settings.ChannelIds.Contains(message.Channel.Id))
            {
                foreach(IMessageProcessor processor in _processors)
                {
                    bool result = await processor.TryProcessAsync(message);

                    if (result)
                    {
                        if(!isWhitelistedUser)
                        {
                            _lastResponse = DateTime.Now;
                        }

                        break;
                    }
                }
            }
        }

        public override Task RegisterAsync()
        {
            Context.Client.MessageReceived += HandleAsync;
            return Task.CompletedTask;
        }
    }
}
