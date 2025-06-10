namespace SonicInflatorService.Core
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T data);
    }
}
