namespace SonicInflatorService.Core
{
    public interface IClientWatcher
    {
        Task WaitForReadyAsync(CancellationToken cancellationToken);
    }
}
