namespace SonicInflatorService.Core.Interfaces
{
    public interface IClientWatcher
    {
        Task WaitForReadyAsync(CancellationToken cancellationToken);
    }
}
