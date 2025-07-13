namespace SonicInflatorService.Core.Interfaces
{
    public interface IInitializable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
