using Discord;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;
using SonicInflatorService.Handlers.EventHandlers;

public class ReadyHandler : EventHandlerBase<ReadyEventArgs>, IClientWatcher
{
    private readonly TaskCompletionSource _tcs = new TaskCompletionSource();

    public Task Ready => _tcs.Task;

    public ReadyHandler(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context)
    {
    }

    public override Task RegisterAsync()
    {
        Context.Client.Ready += HandleAsync;
        return Task.CompletedTask;
    }

    private Task HandleAsync()
    {
        return HandleAsync(new ReadyEventArgs());
    }

    public override Task HandleAsync(ReadyEventArgs args)
    {
        Context.Client.Ready -= HandleAsync;

        try
        {
            Context.Client.SetStatusAsync(UserStatus.Online);
            Logger.LogInformation("Bot status set to Online.");

            _tcs.TrySetResult();
        }
        catch (Exception ex) 
        {
            Logger.LogError(ex, "Failed to set online status.");

            _tcs.TrySetException(ex);
        }

        return Task.CompletedTask;
    }

    public async Task WaitForReadyAsync(CancellationToken cancellationToken)
    {
        using (cancellationToken.Register(() => _tcs.TrySetCanceled(cancellationToken)))
        {
            await _tcs.Task;
        }

    }
}
