using System.Text.RegularExpressions;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public abstract class MessageProcessorBase : IMessageProcessor
    {
        protected readonly Regex _regex;
        protected ILogger Logger { get; }
        protected IBotContext Context { get; }

        public MessageProcessorBase(ILoggerFactory loggerFactory, IBotContext context, string pattern)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            Context = context;
            _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public Task<bool> TryProcessAsync(SocketMessage message)
        {
            bool isMatch = _regex.IsMatch(message.CleanContent);

            if (isMatch)
            {
                ProcessAsync(message);
            }

            return Task.FromResult(isMatch);
        }

        public abstract Task ProcessAsync(SocketMessage message);
    }
}
