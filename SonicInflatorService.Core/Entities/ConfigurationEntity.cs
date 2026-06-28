namespace SonicInflatorService.Core.Entities
{
    public class ConfigurationEntity : BaseEntity
    {
        public required string Key { get; set; }
        public string? Value { get; set; }
        public string? Section { get; set; }
    }
}
