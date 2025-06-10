using Discord;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Infrastructure
{
    public class Bot : BackgroundService
    {
        private readonly ILogger<Bot> _logger;
        private readonly IBotContext _context;
        private readonly IClientWatcher _watcher;
        private readonly IEnumerable<IEventBinding> _bindings;
        private readonly IEnumerable<IInitializable> _initializables;
        private readonly IChannelTracker _tracker;
        private readonly Random _random = new();

        public Bot(ILogger<Bot> logger, IBotContext context, IClientWatcher watcher, IEnumerable<IEventBinding> bindings, IEnumerable<IInitializable> initializables, IChannelTracker tracker)
        {
            _logger = logger;
            _context = context;
            _watcher = watcher;
            _bindings = bindings;
            _initializables = initializables;
            _tracker = tracker;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {            
            await RegisterAllEventBindingsAsync();
            await LoginAndStartAsync(cancellationToken);
            await _watcher.WaitForReadyAsync(cancellationToken);
            await InitializeAllAsync(cancellationToken);
            await DoWorkAsync(cancellationToken);
        }

        private async Task InitializeAllAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_initializables.Select(b => b.InitializeAsync(cancellationToken)));
        }
        private async Task RegisterAllEventBindingsAsync()
        {
            await Task.WhenAll(_bindings.Select(b => b.RegisterAsync()));
        }
        private async Task LoginAndStartAsync(CancellationToken cancellationToken)
        {
            await _context.Client.LoginAsync(TokenType.Bot, _context.Settings.Token);
            await _context.Client.StartAsync();

            _logger.LogInformation("Discord client started.");

        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            ulong channelId = _context.Settings.PrimaryChannelId;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await SendImageAsync(channelId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception was thrown while inflating sonic");
                }
                TimeSpan delay = GetRandomDelay();
                channelId = SelectRandomChannelId();
                _logger.LogInformation($"Waiting {delay.TotalMinutes} minutes before sending next image...");
                await Task.Delay(delay, cancellationToken);
            }
        }

        private async Task SendImageAsync(ulong channelId, CancellationToken cancellationToken)
        {
            try
            {
                IMessageChannel channel = GetDiscordChannel(channelId);
                if(channel == null)
                {
                    _logger.LogWarning($"Channel not found: {channelId}");
                    return;
                }

                string text = "";

                if(channelId != _context.Settings.PrimaryChannelId)
                {
                    //containment breach
                    text = $"<a:{_context.Settings.SirenEmojiName}:{_context.Settings.SirenEmojiId}> CONTAINMENT BREACH <a:{_context.Settings.SirenEmojiName}:{_context.Settings.SirenEmojiId}>";
                }

                _tracker.TrackLastImage(channelId, _context.Settings.InflatedImagePath);
                await channel.SendFileAsync(_context.Settings.InflatedImagePath, text);
                _logger.LogInformation("Image sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send image.");
            }           
        }

        private TimeSpan GetRandomDelay()
        {
            int minutes = _random.Next(_context.Settings.RandomIntervalMinutesMinValue, _context.Settings.RandomIntervalMinutesMaxValue + 1);
            return TimeSpan.FromMinutes(minutes);
        }

        private IMessageChannel GetDiscordChannel(ulong channelId)
        {
            return _context.Client.GetChannel(channelId) as IMessageChannel;
        }

        private ulong SelectRandomChannelId()
        {
            ulong selectedChannelId;
            int randomChannelChancePercentage = _context.Settings.RandomChannelPercentageChance;
            bool randomChannelChance = _random.Next(100) < randomChannelChancePercentage;

            if (randomChannelChance)
            {
                selectedChannelId = _context.Settings.ChannelIds[_random.Next(_context.Settings.ChannelIds.Count)];
                _logger.LogInformation($"[{randomChannelChancePercentage}% chance] Randomly selected channel.");
            }
            else
            {
                selectedChannelId = _context.Settings.PrimaryChannelId;
            }

            return selectedChannelId;
        }
    }
}
