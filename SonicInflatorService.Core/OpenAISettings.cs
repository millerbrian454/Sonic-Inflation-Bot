namespace SonicInflatorService.Core
{
    public class OpenAISettings
    {
        public required string ApiKey { get; set; }
        public required string BaseUri { get; set; }
        public required List<string> Models { get; set; }
    }
}

