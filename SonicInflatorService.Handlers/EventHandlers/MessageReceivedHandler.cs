using System.Threading.Channels;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Handlers.EventHandlers
{
    public class MessageReceivedHandler : EventHandlerBase<SocketMessage>
    {
        private readonly TimeSpan _cooldown;
        private readonly IList<IMessageProcessor> _processors;
        private DateTime _lastResponse;

        private readonly Channel<SocketMessage> _queue;
        private readonly Task _worker;
        private readonly CancellationTokenSource _cts;

        public MessageReceivedHandler(ILoggerFactory loggerFactory, IBotContext context, IList<IMessageProcessor> processors) : base(loggerFactory, context)
        {
            _processors = processors;
            _cooldown = TimeSpan.FromSeconds(Context.Settings.ResponseCooldownIntervalSeconds);

            _queue = Channel.CreateUnbounded<SocketMessage>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

            _cts = new CancellationTokenSource();
            _worker = Task.Run(ProcessQueueAsync);

        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                await foreach (SocketMessage message in _queue.Reader.ReadAllAsync(_cts.Token))
                {
                    await ProcessMessageAsync(message);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing queued message");
            }
        }

        private async Task ProcessMessageAsync(SocketMessage message)
        {
            bool isWhitelistedUser = Context.Settings.ProfessionalSonicWranglerUserIds.Contains(message.Author.Id);

            foreach (IMessageProcessor processor in _processors)
            {
                bool result = await processor.TryProcessAsync(message);

                if (!result) continue;
                if (!isWhitelistedUser)
                {
                    _lastResponse = DateTime.Now;
                }

                break;
            } 
        }

        public override Task HandleAsync(SocketMessage message)
        {
            bool isWhitelistedUser = Context.Settings.ProfessionalSonicWranglerUserIds.Contains(message.Author.Id);

            if ((isWhitelistedUser ||
                _lastResponse.Add(_cooldown) <= DateTime.Now)
                && message.Author.Id != Context.Client.CurrentUser.Id
                && !message.Author.IsBot
                && Context.Settings.ChannelIds.Contains(message.Channel.Id))
            {
                if (!_queue.Writer.TryWrite(message))
                {
                    Logger.LogWarning($"Failed to enqueue message from {message.Author.Username} in {message.Channel.Name}");
                }
            }

            return Task.CompletedTask;
            
        }

        public override Task RegisterAsync()
        {
            Context.Client.MessageReceived += HandleAsync;
            return Task.CompletedTask;
        }
    }
}
