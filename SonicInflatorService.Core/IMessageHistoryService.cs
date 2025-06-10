using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SonicInflatorService.Core
{
    public interface IMessageHistoryService
    {
        Task<IEnumerable<string>> GetRecentMessagesAsync(IGuild guild, IUser user, int limit = 50);
    }
}
