using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SonicInflatorService.Core.Entities;

namespace SonicInflatorService.Infrastructure.Data
{
    public static class ConfigurationSeeder
    {
        public static async Task SeedFromAppSettingsAsync(SonicInflatorDbContext context, IConfiguration configuration)
        {
            // Seed OpenAI Configuration
            var openAiConfig = await context.OpenAIConfigurations.Include(c => c.Models).FirstOrDefaultAsync();
            if (openAiConfig == null)
            {
                var apiKey = configuration["OpenAI:ApiKey"];
                var baseUri = configuration["OpenAI:BaseUri"];
                var models = configuration.GetSection("OpenAI:Models").Get<List<string>>() ?? new List<string>();

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(baseUri))
                {
                    openAiConfig = new OpenAIConfigurationEntity
                    {
                        ApiKey = apiKey,
                        BaseUri = baseUri,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Models = models.Select(m => new OpenAIModelEntity 
                        { 
                            ModelName = m, 
                            CreatedAt = DateTime.UtcNow 
                        }).ToList()
                    };

                    context.OpenAIConfigurations.Add(openAiConfig);
                }
            }

            // Seed Discord Configuration
            var discordConfig = await context.DiscordConfigurations
                .Include(c => c.ChannelIds)
                .Include(c => c.ContextChannelIds)
                .Include(c => c.ProfessionalSonicWranglerUserIds)
                .FirstOrDefaultAsync();

            if (discordConfig == null)
            {
                var token = configuration["Discord:Token"];
                if (!string.IsNullOrEmpty(token))
                {
                    var channelIds = configuration.GetSection("Discord:ChannelIds").Get<List<ulong>>() ?? new List<ulong>();
                    var contextChannelIds = configuration.GetSection("Discord:ContextChannelIds").Get<List<ulong>>() ?? new List<ulong>();
                    var wranglerIds = configuration.GetSection("Discord:ProfessionalSonicWranglerUserIds").Get<List<ulong>>() ?? new List<ulong>();

                    discordConfig = new DiscordConfigurationEntity
                    {
                        Token = token,
                        PrimaryChannelId = configuration.GetValue<ulong>("Discord:PrimaryChannelId"),
                        GuildId = ulong.Parse(configuration["Discord:GuildId"] ?? "0"),
                        MimicUserId = ulong.Parse(configuration["Discord:MimicUserId"] ?? "0"),
                        SirenEmojiId = ulong.Parse(configuration["Discord:SirenEmojiId"] ?? "0"),
                        SirenEmojiName = configuration["Discord:SirenEmojiName"],
                        InflatedImagePath = configuration["Discord:InflatedImagePath"],
                        DeflatedImagePath = configuration["Discord:DeflatedImagePath"],
                        SonichuImagePath = configuration["Discord:SonichuImagePath"],
                        CurseYeHaMeHaImagePath = configuration["Discord:CurseYeHaMeHaImagePath"],
                        RandomIntervalMinutesMaxValue = configuration.GetValue<int>("Discord:RandomIntervalMinutesMaxValue"),
                        RandomIntervalMinutesMinValue = configuration.GetValue<int>("Discord:RandomIntervalMinutesMinValue"),
                        ResponseCooldownIntervalSeconds = configuration.GetValue<int>("Discord:ResponseCooldownIntervalSeconds"),
                        RandomChannelPercentageChance = configuration.GetValue<int>("Discord:RandomChannelPercentageChance"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ChannelIds = channelIds.Select(id => new DiscordChannelEntity { ChannelId = id, CreatedAt = DateTime.UtcNow }).ToList(),
                        ContextChannelIds = contextChannelIds.Select(id => new DiscordContextChannelEntity { ChannelId = id, CreatedAt = DateTime.UtcNow }).ToList(),
                        ProfessionalSonicWranglerUserIds = wranglerIds.Select(id => new DiscordProfessionalWranglerEntity { UserId = id, CreatedAt = DateTime.UtcNow }).ToList()
                    };

                    context.DiscordConfigurations.Add(discordConfig);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}

