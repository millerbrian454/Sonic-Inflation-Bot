namespace SonicInflatorService.Core
{
    public interface ILlmService
    {
        Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt);
    }
}
