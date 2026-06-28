using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core.Entities;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Infrastructure.Services
{
    public class FallbackConfigurationService : IConfigurationService
    {
        private readonly DatabaseConfigurationService _databaseService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FallbackConfigurationService> _logger;
        private bool _databaseAvailable = true;

        public FallbackConfigurationService(
            DatabaseConfigurationService databaseService,
            IConfiguration configuration,
            ILogger<FallbackConfigurationService> logger)
        {
            _databaseService = databaseService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OpenAIConfigurationEntity?> GetOpenAIConfigurationAsync()
        {
            if (_databaseAvailable)
            {
                try
                {
                    var result = await _databaseService.GetOpenAIConfigurationAsync();
                    if (result != null)
                        return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Database connection failed, falling back to appsettings.json for OpenAI configuration");
                    _databaseAvailable = false;
                }
            }

            // Fallback to appsettings.json
            return GetOpenAIConfigurationFromAppSettings();
        }

        public async Task<OllamaConfigurationEntity?> GetOllamaConfigurationAsync()
        {
            var baseUri = _configuration["OllamaConfig:OllamaBaseUri"];
            var shouldUseOllama = _configuration.GetValue<bool>("OllamaConfig:UseOllama");
            
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                _logger.LogWarning("Ollama configuration not found in appsettings.json");
                return null;
            }

            var models = _configuration.GetSection("OllamaConfig:OllamaModels").Get<List<string>>() ?? new List<string>();

            _logger.LogInformation("Using Ollama configuration from appsettings.json");

            return new OllamaConfigurationEntity()
            {
                Id = -1,
                BaseUri = baseUri,
                Models = models.Select(m => new OllamaModelEntity()
                {
                    Id = -1,
                    ModelName = m,
                    CreatedAt = DateTime.UtcNow
                }).ToList(),
                ShouldUseLlm = shouldUseOllama
            };
        }

        public async Task<DiscordConfigurationEntity?> GetDiscordConfigurationAsync()
        {
            if (_databaseAvailable)
            {
                try
                {
                    var result = await _databaseService.GetDiscordConfigurationAsync();
                    if (result != null)
                        return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Database connection failed, falling back to appsettings.json for Discord configuration");
                    _databaseAvailable = false;
                }
            }

            // Fallback to appsettings.json
            return GetDiscordConfigurationFromAppSettings();
        }

        public async Task<string?> GetConfigurationValueAsync(string key, string? section = null)
        {
            if (_databaseAvailable)
            {
                try
                {
                    return await _databaseService.GetConfigurationValueAsync(key, section);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Database connection failed for configuration value retrieval, falling back to appsettings.json");
                    _databaseAvailable = false;
                }
            }

            // Fallback to appsettings.json
            var configPath = string.IsNullOrEmpty(section) ? key : $"{section}:{key}";
            return _configuration[configPath];
        }

        private OpenAIConfigurationEntity? GetOpenAIConfigurationFromAppSettings()
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var baseUri = _configuration["OpenAI:BaseUri"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUri))
            {
                _logger.LogWarning("OpenAI configuration not found in appsettings.json");
                return null;
            }

            var models = _configuration.GetSection("OpenAI:Models").Get<List<string>>() ?? new List<string>();

            _logger.LogInformation("Using OpenAI configuration from appsettings.json");

            return new OpenAIConfigurationEntity
            {
                Id = -1, // Indicate this is from appsettings
                ApiKey = apiKey,
                BaseUri = baseUri,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Models = models.Select(m => new OpenAIModelEntity
                {
                    Id = -1,
                    ModelName = m,
                    CreatedAt = DateTime.UtcNow,
                    OpenAIConfigurationId = -1
                }).ToList()
            };
        }

        private DiscordConfigurationEntity? GetDiscordConfigurationFromAppSettings()
        {
            var token = _configuration["Discord:Token"];

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Discord Token not found in appsettings.json");
                return null;
            }

            var channelIds = _configuration.GetSection("Discord:ChannelIds").Get<List<ulong>>() ?? new List<ulong>();
            var contextChannelIds = _configuration.GetSection("Discord:ContextChannelIds").Get<List<ulong>>() ?? new List<ulong>();
            var wranglerIds = _configuration.GetSection("Discord:ProfessionalSonicWranglerUserIds").Get<List<ulong>>() ?? new List<ulong>();
            var naughtyWords = _configuration.GetSection("Discord:NaughtyWords").Get<List<string>>() ?? new List<string>();

            _logger.LogInformation("Using Discord configuration from appsettings.json");

            return new DiscordConfigurationEntity
            {
                Id = -1, // Indicate this is from appsettings
                Token = token,
                PrimaryChannelId = _configuration.GetValue<ulong>("Discord:PrimaryChannelId"),
                GuildId = _configuration.GetValue<ulong>("Discord:GuildId"),
                MimicUserId = _configuration.GetValue<ulong>("Discord:MimicUserId"),
                SirenEmojiId = _configuration.GetValue<ulong>("Discord:SirenEmojiId"),
                SirenEmojiName = _configuration["Discord:SirenEmojiName"] ?? "🚨",
                InflatedImagePath = _configuration["Discord:InflatedImagePath"],
                DeflatedImagePath = _configuration["Discord:DeflatedImagePath"],
                SonichuImagePath = _configuration["Discord:SonichuImagePath"],
                CurseYeHaMeHaImagePath = _configuration["Discord:CurseYeHaMeHaImagePath"],
                RandomIntervalMinutesMaxValue = _configuration.GetValue<int>("Discord:RandomIntervalMinutesMaxValue"),
                RandomIntervalMinutesMinValue = _configuration.GetValue<int>("Discord:RandomIntervalMinutesMinValue"),
                ResponseCooldownIntervalSeconds = _configuration.GetValue<int>("Discord:ResponseCooldownIntervalSeconds"),
                RandomChannelPercentageChance = _configuration.GetValue<int>("Discord:RandomChannelPercentageChance"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ChannelIds = channelIds.Select(id => new DiscordChannelEntity
                {
                    Id = -1,
                    ChannelId = id,
                    CreatedAt = DateTime.UtcNow,
                    DiscordConfigurationId = -1
                }).ToList(),
                ContextChannelIds = contextChannelIds.Select(id => new DiscordContextChannelEntity
                {
                    Id = -1,
                    ChannelId = id,
                    CreatedAt = DateTime.UtcNow,
                    DiscordConfigurationId = -1
                }).ToList(),
                ProfessionalSonicWranglerUserIds = wranglerIds.Select(id => new DiscordProfessionalWranglerEntity
                {
                    Id = -1,
                    UserId = id,
                    CreatedAt = DateTime.UtcNow,
                    DiscordConfigurationId = -1
                }).ToList(),
                NaughtyWords = naughtyWords.Select(id => new DiscordNaughtyWordEntity
                {
                    Id = -1,
                    NaughtyWord = id,
                    CreatedAt = DateTime.UtcNow,
                    DiscordConfigurationId = -1
                }).ToList()
            };
        }
    }
}