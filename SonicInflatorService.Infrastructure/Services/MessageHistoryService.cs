using Discord;
using Discord.WebSocket;
using Serilog;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Infrastructure.Services
{
    public class MessageHistoryService : IMessageHistoryService
    {
        private readonly IBotContext _context;

        public MessageHistoryService(IBotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetRecentMessagesAsync(ITextChannel channel, int limit = 100)
        {
            try
            {

                IEnumerable<IMessage> messages = await channel.GetMessagesAsync(limit: limit).FlattenAsync();

                //TODO: Pull ternary operation out of linq statement into its own method
                var filteredMessages = messages 
                    .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                    .Distinct()
                    .OrderBy(m => m.Timestamp)
                    .Take(limit)
                    .Select(m =>
                    {
                        var author = m.Author as SocketGuildUser;
                        return author != null ? $"{author.DisplayName}: {m.CleanContent}" : $"Unknown: {m.CleanContent}";
                    })                    .ToList();

                return filteredMessages;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception was thrown while getting the recent messages");
                return new List<string>();
            }

        }
        public async Task<IEnumerable<string>> GetRecentMessagesAsync(IGuild guild, IUser user, int limit = 100)
        {
            HashSet<string> result = new HashSet<string>();

            foreach(ulong channelId in _context.Settings.ContextChannelIds)
            {
                ITextChannel channel = (await guild.GetChannelAsync(channelId)) as ITextChannel;

                if (channel == null) continue;

                try
                {
                    IEnumerable<IMessage> messages = await channel.GetMessagesAsync(limit: 100).FlattenAsync();

                    IEnumerable<string> userMessages = messages
                        .Where(m => m.Author.Id == user.Id && !string.IsNullOrWhiteSpace(m.Content))
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.CleanContent);

                    foreach (string msg in userMessages)
                    {
                        result.Add(msg);

                        if (result.Count >= limit)
                            return result;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return result.ToList();
        }
    }
}
