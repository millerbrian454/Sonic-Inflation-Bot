using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace SonicInflatorService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DiscordSocketClient _client;
        private readonly DiscordSettings? _settings;
        private IMessageChannel? _discordChannel;
        private readonly Random _random = new();
        private readonly TaskCompletionSource _tcs;
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _tcs = new TaskCompletionSource();
            DiscordSocketConfig socketConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(socketConfig);
            _settings = config.GetSection("Discord").Get<DiscordSettings>();
            _client.Log += LogDiscordResponseMessage;
            _client.Ready += GetPrimaryDiscordChannel;
            _client.MessageReceived += GetDiscordChannelMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_settings == null)
                {
                    _logger.LogError("Configuration settings for discord were not initialized properly.");
                    return;
                }

                await _client.LoginAsync(TokenType.Bot, _settings.Token);
                await _client.StartAsync();

                using (stoppingToken.Register(() => _tcs.TrySetCanceled()))
                {
                    await _tcs.Task;
                }

                if (_discordChannel != null)
                {
                    try
                    {
                        await SendImageOnRandomIntervalAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception was thrown while inflating sonic");
                    }
                }
            }
        }

        private Task GetPrimaryDiscordChannel()
        {
            _discordChannel = _client.GetChannel(_settings.PrimaryChannelId) as IMessageChannel;

            if (_discordChannel == null)
            { 
                _logger.LogError("Primary channel not found.");
                _tcs.TrySetCanceled();
            }
            else
            {
                _logger.LogInformation("Bot is ready. Channel acquired.");
                _tcs.TrySetResult();
            }
            return Task.CompletedTask;
        }

        private static readonly Regex _sonicMentioned = new Regex(@"\b(sonic|inflat\w*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private async Task GetDiscordChannelMessage(SocketMessage message)
        {
            if(message.Author.Id != _client.CurrentUser.Id 
                && !message.Author.IsBot
                && _settings != null
                && _settings.ChannelIds.Contains(message.Channel.Id)
                && _sonicMentioned.IsMatch(message.Content))
            {
                if (_client.GetChannel(message.Channel.Id) is IMessageChannel channel)
                {
                    await channel.SendFileAsync(_settings.ImagePath, $"DID {message.Author.Mention} SAY SONIC INFLATION?!");
                }

            }
        }

        private async Task SendImageOnRandomIntervalAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _discordChannel.SendFileAsync(_settings.ImagePath);
                _logger.LogInformation("Image sent immediately on startup.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send initial image.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                int delayMinutes =  _random.Next(_settings.RandomIntervalMinutesMinValue, _settings.RandomIntervalMinutesMaxValue + 1);
                int delayMilliseconds = delayMinutes * 60 * 1000;
                
                ulong selectedChannelId = SelectRandomChannelId();
                _discordChannel = _client.GetChannel(selectedChannelId) as IMessageChannel;

                _logger.LogInformation($"Waiting {delayMinutes} minutes before sending next image...");
                await Task.Delay(delayMilliseconds, stoppingToken);

                try
                {
                    await _discordChannel.SendFileAsync(_settings.ImagePath);
                    _logger.LogInformation("Image sent successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send image.");
                }
            }
        }

        private Task LogDiscordResponseMessage(LogMessage discordLogMessage)
        {
            if (discordLogMessage.Exception is CommandException cmdException)
            {
                _logger.LogError($"[Command/{discordLogMessage.Severity}] {cmdException.Command.Aliases.First()}"
                         + $" failed to execute in {cmdException.Context.Channel}.");
            }
            else
            {
                _logger.LogInformation($"Discord API Response: [{discordLogMessage.Severity}]{discordLogMessage}");
            }
                

            return Task.CompletedTask;
        }

        private ulong SelectRandomChannelId()
        {
            ulong selectedChannelId;
            int randomChannelChancePercentage = _settings.RandomChannelPercentageChance;
            bool randomChannelChance = _random.Next(100) < randomChannelChancePercentage;

            if (randomChannelChance)
            {
                selectedChannelId = _settings.ChannelIds[_random.Next(_settings.ChannelIds.Count)];
                _logger.LogInformation($"[{randomChannelChancePercentage}% chance] Randomly selected channel.");
            }
            else
            {
                selectedChannelId = _settings.PrimaryChannelId;
            }

            return selectedChannelId;
        }
    }
}
