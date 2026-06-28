using SonicInflatorService.Core.Entities;

namespace SonicInflatorService.Core.Interfaces
{
    public interface IConfigurationService
    {
        Task<OpenAIConfigurationEntity?> GetOpenAIConfigurationAsync();
        Task<DiscordConfigurationEntity?> GetDiscordConfigurationAsync();
        Task<OllamaConfigurationEntity?> GetOllamaConfigurationAsync();
    }
}
