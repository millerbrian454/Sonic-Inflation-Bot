namespace SonicInflatorService.Core
{
    public interface IInitializable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
