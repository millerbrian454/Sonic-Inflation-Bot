using SonicInflatorService.Core.Entities;

namespace SonicInflatorService.Infrastructure.Extensions;

public static class EntityExtensions
{
    public static void CheckModelCount(this List<string> models)
    {
        if (models.Count == 0)
        {
            throw new InvalidOperationException("No OpenAI models configured");
        }
    }
    
    public static void CheckConfigForNull(this BaseEntity config)
    {
        if (config == null)
        {
            throw new InvalidOperationException("OpenAI configuration not found in database");
        }
    }
}