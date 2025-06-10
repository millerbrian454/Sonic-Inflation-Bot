using Autofac;
using Discord.WebSocket;
using Discord;
using SonicInflatorService.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Module = Autofac.Module;
using Microsoft.Extensions.Hosting;
using SonicInflatorService.Infrastructure;

namespace SonicInflatorService.DependencyInjection
{
    public class BotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
            .Register(context => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
                | GatewayIntents.MessageContent
                | GatewayIntents.Guilds
                | GatewayIntents.GuildMessages
                | GatewayIntents.GuildMembers
            })).AsSelf().SingleInstance();

            builder
            .Register(ctx =>
            {
                var config = ctx.Resolve<IConfiguration>();
                var services = new ServiceCollection();
                services.AddOptions();
                // Add options and config binding with reloadOnChange = true
                services.Configure<DiscordSettings>(config.GetSection("Discord"));

                // Build service provider to get IOptionsMonitor<T>
                var sp = services.BuildServiceProvider();
                return sp.GetRequiredService<IOptionsMonitor<DiscordSettings>>();
            })
            .As<IOptionsMonitor<DiscordSettings>>()
            .SingleInstance();

            builder
            .RegisterType<BotContext>()
            .As<IBotContext>()
            .SingleInstance();

            builder
            .RegisterType<Bot>()
            .As<IHostedService>()
            .SingleInstance();      
        }
    }
}
