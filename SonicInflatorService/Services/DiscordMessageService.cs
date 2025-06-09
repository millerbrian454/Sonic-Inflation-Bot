using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace SonicInflatorService.Services
{
    public interface IDiscordMessageService
    {
        Task LogDiscordResponseMessage(LogMessage discordLogMessage);
        bool IsValidMessage(SocketMessage message, DiscordSettings settings, DateTime lastResponse, TimeSpan coolDown, ulong currentUserId);
    }
    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly ILogger<DiscordMessageService> _logger;

        public DiscordMessageService(ILogger<DiscordMessageService> logger)
        {
            _logger = logger;
        }

        public bool IsValidMessage(SocketMessage message, DiscordSettings settings, DateTime lastResponse, TimeSpan coolDown, ulong currentClientUserId)
        {
            bool isWhitelistedUser = settings.ProfessionalSonicWranglerUserIds.Contains(message.Author.Id);

            return (isWhitelistedUser || lastResponse.Add(coolDown) <= DateTime.Now)
                   && message.Author.Id != currentClientUserId
                   && !message.Author.IsBot
                   && settings.ChannelIds.Contains(message.Channel.Id);
        }

        public Task LogDiscordResponseMessage(LogMessage discordLogMessage)
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
    }
}
