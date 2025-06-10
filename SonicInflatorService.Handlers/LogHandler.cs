using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers
{
    public class LogHandler : EventHandlerBase<LogMessage>
    {
        public LogHandler(ILoggerFactory loggerFactory, IBotContext context) : base(loggerFactory, context)
        {
        }

        public override Task HandleAsync(LogMessage message)
        {
            LogLevel logLevel = message.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => LogLevel.Information
            };

            string logMessage;
            if (message.Exception is CommandException cmdException)
            {
                logMessage = $"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                         + $" failed to execute in {cmdException.Context.Channel}.";
            }
            else
            {
               logMessage = $"Discord API Response: {message.Message}";
            }

            Logger.Log(logLevel, message.Exception, logMessage, message.Source, message.Message);


            return Task.CompletedTask;
        }

        public override Task RegisterAsync()
        {
            Context.Client.Log += HandleAsync;
            return Task.CompletedTask;
        }
    }
}
