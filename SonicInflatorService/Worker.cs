using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using SonicInflatorService.Services;

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
        private readonly TimeSpan _cooldown;
        private DateTime _lastResponse;
        private readonly IDiscordMessageService _messageService;
        private readonly IDiscordChannelService _channelService;
        public Worker(ILogger<Worker> logger, IConfiguration config, IDiscordMessageService messageService, IDiscordChannelService channelService)
        {
            _logger = logger;
            _messageService = messageService;
            _channelService = channelService;
            _tcs = new TaskCompletionSource();
            DiscordSocketConfig socketConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(socketConfig);
            _settings = config.GetSection("Discord").Get<DiscordSettings>();
            _cooldown = TimeSpan.FromSeconds(_settings.ResponseCooldownIntervalSeconds);
            _lastResponse = DateTime.Now.AddSeconds(-_settings.ResponseCooldownIntervalSeconds);
            _client.Log += _messageService.LogDiscordResponseMessage;
            _client.Ready += async () =>
            {
                await _client.SetStatusAsync(UserStatus.Online);
                _logger.LogInformation("Bot status set to Online.");
                await GetPrimaryDiscordChannel();
            };
            _client.MessageReceived += GetDiscordChannelMessage;
            _channelService = channelService;
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

                if (_discordChannel == null) continue;
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

        private static readonly Regex SonicMentioned = new Regex(@"\b(sonic|inflat\w*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SanicMentioned = new Regex(@"\b(sanic|sonichu\w*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private async Task GetDiscordChannelMessage(SocketMessage message)
        {
            try
            {
                if (_settings != null && _messageService.IsValidMessage(message, _settings, _lastResponse, _cooldown, _client.CurrentUser.Id))
                {
                    if (_client.GetChannel(message.Channel.Id) is IMessageChannel channel)
                    {
                        if (SonicMentioned.IsMatch(message.Content))
                        {
                            await channel.SendFileAsync(_settings.InflatedImagePath,
                                $"DID {message.Author.Mention} SAY SONIC INFLATION?!");
                            _lastResponse = DateTime.Now;
                        }
                        else if (message.CleanContent == "ALAKAGOO! 👉")
                        {
                            await channel.SendFileAsync(_settings.DeflatedImagePath,
                                $"SONIC NOOOOOOOOO. {message.Author.Mention} WHAT HAVE YOU DONE?!");
                            _lastResponse = DateTime.Now;
                        }
                        else if (SanicMentioned.IsMatch(message.Content))
                        {
                            await channel.SendFileAsync(_settings.SonichuPathToDamnation,
                                $"OH NO. {message.Author.Mention} HAS SUMMONED SANIC!");
                            _lastResponse = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown while attempting to respond to a user");
            }
        }

        private async Task SendImageOnRandomIntervalAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _discordChannel.SendFileAsync(_settings.InflatedImagePath);
                _logger.LogInformation("Image sent immediately on startup.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send initial image.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int delayMinutes =  _random.Next(_settings.RandomIntervalMinutesMinValue, _settings.RandomIntervalMinutesMaxValue + 1);
                    int delayMilliseconds = delayMinutes * 60 * 1000;
                
                    ulong selectedChannelId = _channelService.SelectRandomChannelId(_settings, _random);
                    _discordChannel = _client.GetChannel(selectedChannelId) as IMessageChannel;

                    _logger.LogInformation($"Waiting {delayMinutes} minutes before sending next image...");

                    await Task.Delay(delayMilliseconds, stoppingToken);
                    IUserMessage? fileSendResult;
                    if (_discordChannel.Id != _settings.PrimaryChannelId)
                    {
                        string containmentBreachAlert = $"<a:{_settings.SirenEmojiName}:{_settings.SirenEmojiId}> CONTAINMENT BREACH <a:{_settings.SirenEmojiName}:{_settings.SirenEmojiId}>";
                        fileSendResult = await _discordChannel.SendFileAsync(_settings.InflatedImagePath, containmentBreachAlert);
                    }
                    else
                    {
                        fileSendResult = await _discordChannel.SendFileAsync(_settings.InflatedImagePath);
                    }
                    _logger.LogInformation("Sonic inflated successfully in channel: {0}", fileSendResult.Channel.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send image.");
                }
            }
        }
    }
}
