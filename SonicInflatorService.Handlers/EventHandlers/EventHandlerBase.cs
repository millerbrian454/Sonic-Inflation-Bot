using Microsoft.Extensions.Logging;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Handlers.EventHandlers
{
    public abstract class EventHandlerBase<T> : IEventHandler<T>, IEventBinding
    {
        protected ILogger Logger { get; }
        protected IBotContext Context { get; }

        protected EventHandlerBase(ILoggerFactory loggerFactory, IBotContext context)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            Context = context;
        }
        public abstract Task HandleAsync(T data);
        public abstract Task RegisterAsync();
    }
}
