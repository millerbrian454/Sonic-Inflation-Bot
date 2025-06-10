using Discord;

namespace SonicInflatorService.Core
{
    public interface IMessageHistoryService
    {
        Task<IEnumerable<string>> GetRecentMessagesAsync(IGuild guild, IUser user, int limit = 50);
        Task<IEnumerable<string>> GetRecentMessagesAsync(ITextChannel channel, int limit = 50);
    }
}
