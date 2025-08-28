using Autofac;
using Discord.WebSocket;
using Discord;
using SonicInflatorService.Core;
using Module = Autofac.Module;
using Microsoft.Extensions.Hosting;
using SonicInflatorService.Infrastructure;
using SonicInflatorService.Core.Interfaces;
using Microsoft.Extensions.Logging;

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
                             var configService = ctx.Resolve<IConfigurationService>();
                             var logger = ctx.Resolve<ILogger<BotModule>>();

                             // Create a wrapper that loads settings synchronously when needed
                             return new Func<DiscordSettings>(() =>
                             {
                                 try
                                 {
                                     var discordConfig = configService.GetDiscordConfigurationAsync().GetAwaiter().GetResult();

                                     if (discordConfig == null)
                                     {
                                         logger.LogError("Discord configuration not found in database or appsettings.json");
                                         throw new InvalidOperationException("Discord configuration not found in database or appsettings.json");
                                     }

                                     logger.LogInformation("Successfully loaded Discord configuration from {Source}",
                                         discordConfig.Id == -1 ? "appsettings.json" : "database");

                                     return new DiscordSettings
                                     {
                                         Token = discordConfig.Token,
                                         PrimaryChannelId = discordConfig.PrimaryChannelId,
                                         GuildId = discordConfig.GuildId,
                                         MimicUserId = discordConfig.MimicUserId,
                                         SirenEmojiId = discordConfig.SirenEmojiId,
                                         SirenEmojiName = discordConfig.SirenEmojiName ?? "🚨",
                                         ChannelIds = discordConfig.ChannelIds.Select(c => c.ChannelId).ToList(),
                                         ContextChannelIds = discordConfig.ContextChannelIds.Select(c => c.ChannelId).ToList(),
                                         InflatedImagePath = discordConfig.InflatedImagePath,
                                         DeflatedImagePath = discordConfig.DeflatedImagePath,
                                         SonichuImagePath = discordConfig.SonichuImagePath,
                                         CurseYeHaMeHaImagePath = discordConfig.CurseYeHaMeHaImagePath,
                                         RandomIntervalMinutesMaxValue = discordConfig.RandomIntervalMinutesMaxValue,
                                         RandomIntervalMinutesMinValue = discordConfig.RandomIntervalMinutesMinValue,
                                         ResponseCooldownIntervalSeconds = discordConfig.ResponseCooldownIntervalSeconds,
                                         RandomChannelPercentageChance = discordConfig.RandomChannelPercentageChance,
                                         ProfessionalSonicWranglerUserIds = discordConfig.ProfessionalSonicWranglerUserIds.Select(p => p.UserId).ToList()
                                     };
                                 }
                                 catch (Exception ex)
                                 {
                                     logger.LogError(ex, "Failed to load Discord configuration from both database and appsettings.json");
                                     throw;
                                 }
                             });
                         })
                         .As<Func<DiscordSettings>>()
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