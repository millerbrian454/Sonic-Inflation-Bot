namespace SonicInflatorService.Core.Interfaces
{
    public interface ILlmService
    {
        Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt);
    }
}
