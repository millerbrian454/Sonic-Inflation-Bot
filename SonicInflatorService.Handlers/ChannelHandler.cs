using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;
using Discord;
using Discord.WebSocket;

namespace SonicInflatorService.Handlers
{
    public class ChannelHandler : EventHandlerBase<ChannelPermissions>
    {
        public ChannelHandler(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context)
        {
        }

        public override async Task HandleAsync(ChannelPermissions channelPermissions)
        {
            var visibleChannelIds = new HashSet<ulong>();

            foreach (SocketGuild discordServer in Context.Client.Guilds)
            {
                foreach (SocketTextChannel channel in discordServer.TextChannels)
                {
                    SocketGuildUser botUserProfile = discordServer.GetUser(Context.Client.CurrentUser.Id);
                    ChannelPermissions? permissions = botUserProfile.GetPermissions(channel);
                    if (permissions is { ViewChannel: true, SendMessages: true, AttachFiles: true })
                    {
                        visibleChannelIds.Add(channel.Id);
                    }
                }
                //TODO save visible channels for each server to db before proceeding to the next discord server
            }
        }

        public override Task RegisterAsync()
        {
            throw new NotImplementedException();
        }
    }
}
