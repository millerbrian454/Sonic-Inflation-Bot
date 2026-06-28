namespace SonicInflatorService.Core.Entities;

public abstract class AiModelBaseEntity
{
    public int Id { get; set; }
    public required string ModelName { get; set; }
    public DateTime CreatedAt { get; set; }
}