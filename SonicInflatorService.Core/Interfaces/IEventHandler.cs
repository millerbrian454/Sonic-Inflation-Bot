namespace SonicInflatorService.Core.Interfaces
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T data);
    }
}
