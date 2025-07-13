using SonicInflatorService.Core.Entities;

namespace SonicInflatorService.Core.Interfaces
{
    public interface IConfigurationService
    {
        Task<OpenAIConfigurationEntity?> GetOpenAIConfigurationAsync();
        Task<DiscordConfigurationEntity?> GetDiscordConfigurationAsync();
        Task<string?> GetConfigurationValueAsync(string key, string? section = null);
        Task SetConfigurationValueAsync(string key, string value, string? section = null);
        Task UpdateOpenAIConfigurationAsync(OpenAIConfigurationEntity configuration);
        Task UpdateDiscordConfigurationAsync(DiscordConfigurationEntity configuration);
    }
}
