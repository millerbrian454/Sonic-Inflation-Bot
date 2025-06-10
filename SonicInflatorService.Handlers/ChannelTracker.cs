using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers
{
    public class ChannelTracker : IChannelTracker
    {
        private readonly ConcurrentDictionary<ulong, string> _lastImages = new ConcurrentDictionary<ulong, string>();

        public string? GetLastImage(ulong channelId)
        {
            _lastImages.TryGetValue(channelId, out string? image);

            return image;
        }

        public void TrackLastImage(ulong channelId, string imagePath)
        {
            _lastImages[channelId] = imagePath;
        }
    }
}
