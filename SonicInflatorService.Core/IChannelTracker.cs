using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;

namespace SonicInflatorService.Core
{
    public interface IChannelTracker
    {
        void TrackLastImage(ulong channelId, string imagePath);
        string? GetLastImage(ulong channelId);
    }
}
