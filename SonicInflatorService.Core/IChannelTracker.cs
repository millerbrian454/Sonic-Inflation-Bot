namespace SonicInflatorService.Core
{
    public interface IChannelTracker
    {
        void TrackLastImage(ulong channelId, string imagePath);
        string? GetLastImage(ulong channelId);
    }
}
