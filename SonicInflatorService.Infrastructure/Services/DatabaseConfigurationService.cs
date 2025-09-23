using Microsoft.EntityFrameworkCore;
using SonicInflatorService.Core.Entities;
using SonicInflatorService.Core.Interfaces;
using SonicInflatorService.Infrastructure.Data;

namespace SonicInflatorService.Infrastructure.Services
{
    public class DatabaseConfigurationService : IConfigurationService
    {
        private readonly SonicInflatorDbContext _context;

        public DatabaseConfigurationService(SonicInflatorDbContext context)
        {
            _context = context;
        }

        public async Task<OpenAIConfigurationEntity?> GetOpenAIConfigurationAsync()
        {
            return await _context.OpenAIConfigurations
                .Include(c => c.Models)
                .FirstOrDefaultAsync();
        }

        public async Task<DiscordConfigurationEntity?> GetDiscordConfigurationAsync()
        {
            return await _context.DiscordConfigurations
                .Include(c => c.ChannelIds)
                .Include(c => c.ContextChannelIds)
                .Include(c => c.ProfessionalSonicWranglerUserIds)
                .Include(c => c.NaughtyWords)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetConfigurationValueAsync(string key, string? section = null)
        {
            var config = await _context.Configurations
                .FirstOrDefaultAsync(c => c.Key == key && c.Section == section);
            return config?.Value;
        }

        public async Task SetConfigurationValueAsync(string key, string value, string? section = null)
        {
            var existing = await _context.Configurations
                .FirstOrDefaultAsync(c => c.Key == key && c.Section == section);

            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Configurations.Add(new ConfigurationEntity
                {
                    Key = key,
                    Value = value,
                    Section = section,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateOpenAIConfigurationAsync(OpenAIConfigurationEntity configuration)
        {
            var existing = await GetOpenAIConfigurationAsync();
            if (existing != null)
            {
                existing.ApiKey = configuration.ApiKey;
                existing.BaseUri = configuration.BaseUri;
                existing.UpdatedAt = DateTime.UtcNow;

                // Update models
                _context.OpenAIModels.RemoveRange(existing.Models);
                existing.Models = configuration.Models;
            }
            else
            {
                configuration.CreatedAt = DateTime.UtcNow;
                configuration.UpdatedAt = DateTime.UtcNow;
                _context.OpenAIConfigurations.Add(configuration);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDiscordConfigurationAsync(DiscordConfigurationEntity configuration)
        {
            var existing = await GetDiscordConfigurationAsync();
            if (existing != null)
            {
                // Update properties
                existing.Token = configuration.Token;
                existing.PrimaryChannelId = configuration.PrimaryChannelId;
                existing.GuildId = configuration.GuildId;
                existing.MimicUserId = configuration.MimicUserId;
                existing.SirenEmojiId = configuration.SirenEmojiId;
                existing.SirenEmojiName = configuration.SirenEmojiName;
                existing.InflatedImagePath = configuration.InflatedImagePath;
                existing.DeflatedImagePath = configuration.DeflatedImagePath;
                existing.SonichuImagePath = configuration.SonichuImagePath;
                existing.CurseYeHaMeHaImagePath = configuration.CurseYeHaMeHaImagePath;
                existing.RandomIntervalMinutesMaxValue = configuration.RandomIntervalMinutesMaxValue;
                existing.RandomIntervalMinutesMinValue = configuration.RandomIntervalMinutesMinValue;
                existing.ResponseCooldownIntervalSeconds = configuration.ResponseCooldownIntervalSeconds;
                existing.RandomChannelPercentageChance = configuration.RandomChannelPercentageChance;
                existing.UpdatedAt = DateTime.UtcNow;

                // Update collections
                _context.DiscordChannels.RemoveRange(existing.ChannelIds);
                _context.DiscordContextChannels.RemoveRange(existing.ContextChannelIds);
                _context.DiscordProfessionalWranglers.RemoveRange(existing.ProfessionalSonicWranglerUserIds);
                _context.DiscordNaughtyWords.RemoveRange(existing.NaughtyWords);

                existing.ChannelIds = configuration.ChannelIds;
                existing.ContextChannelIds = configuration.ContextChannelIds;
                existing.ProfessionalSonicWranglerUserIds = configuration.ProfessionalSonicWranglerUserIds;
                existing.NaughtyWords = configuration.NaughtyWords;
            }
            else
            {
                configuration.CreatedAt = DateTime.UtcNow;
                configuration.UpdatedAt = DateTime.UtcNow;
                _context.DiscordConfigurations.Add(configuration);
            }

            await _context.SaveChangesAsync();
        }
    }
}
