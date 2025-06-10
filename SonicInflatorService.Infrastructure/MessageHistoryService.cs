using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SonicInflatorService.Core;

namespace SonicInflatorService.Infrastructure
{
    public class MessageHistoryService : IMessageHistoryService
    {
        public async Task<IEnumerable<string>> GetRecentMessagesAsync(ITextChannel channel, int limit = 50)
        {
            List<string> result = new List<string>();
           
            IEnumerable<IMessage> messages = await channel.GetMessagesAsync(limit: limit).FlattenAsync();

            result.AddRange(messages
                .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                .OrderBy(m => m.Timestamp)
                .Take(limit)
                .Select(m => $"{(m.Author as SocketGuildUser).DisplayName}: {m.Content}"));            

            return result;
        }
        public async Task<IEnumerable<string>> GetRecentMessagesAsync(IGuild guild, IUser user, int limit = 50)
        {
            List<string> result = new List<string>();
            IReadOnlyCollection<ITextChannel> textChannels = await guild.GetTextChannelsAsync();

            foreach (ITextChannel channel in textChannels)
            {
                try
                {
                    IEnumerable<IMessage> messages = await channel.GetMessagesAsync(limit: 100).FlattenAsync();

                    IEnumerable<string> userMessages = messages
                        .Where(m => m.Author.Id == user.Id && !string.IsNullOrWhiteSpace(m.Content))
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Content);

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

            return result;
        }
    }
}
