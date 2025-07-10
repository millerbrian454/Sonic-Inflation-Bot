namespace SonicInflatorService.Core.Entities
{
    public class ConfigurationEntity
    {
        public int Id { get; set; }
        public required string Key { get; set; }
        public string? Value { get; set; }
        public string? Section { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
