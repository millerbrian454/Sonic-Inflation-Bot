using Discord;
using Discord.WebSocket;
using Serilog;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Infrastructure
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

                return messages
                    .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                    .Distinct()
                    .OrderBy(m => m.Timestamp)
                    .Take(limit)
                    .Select(m =>$"{(m.Author as SocketGuildUser).DisplayName}: {m.CleanContent}")
                    .ToList();

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
