using System.Text.RegularExpressions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers
{
    public class MessageReceivedHandler : EventHandlerBase<SocketMessage>
    {
        private readonly TimeSpan _cooldown;
        private DateTime _lastResponse;
        private static readonly Regex _sonicMentioned = new Regex(@"\b(sonic|inflat\w*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public MessageReceivedHandler(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context)
        {
        }

        public async override Task HandleAsync(SocketMessage message)
        {
            if (_lastResponse.Add(_cooldown) <= DateTime.Now
                && message.Author.Id != Context.Client.CurrentUser.Id
            && !message.Author.IsBot
            && Context.Settings.ChannelIds.Contains(message.Channel.Id))
            {
                
                if (_sonicMentioned.IsMatch(message.Content))
                {
                    await message.Channel.SendFileAsync(Context.Settings.InflatedImagePath, $"DID {message.Author.Mention} SAY SONIC INFLATION?!");
                    _lastResponse = DateTime.Now;
                }
                else if (message.CleanContent == "ALAKAGOO! 👉")
                {
                    await message.Channel.SendFileAsync(Context.Settings.DeflatedImagePath, $"SONIC NOOOOOOOOO. {message.Author.Mention} WHAT HAVE YOU DONE?!");
                    _lastResponse = DateTime.Now;
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
