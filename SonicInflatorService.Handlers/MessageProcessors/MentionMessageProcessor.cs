using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class MentionMessageProcessor : IMessageProcessor, IInitializable
    {
        private readonly IBotContext _context;
        private readonly IMessageHistoryService _historyService;
        private readonly ILlmService _llm;
        private readonly ILogger<MentionMessageProcessor> _logger;
        private SocketGuild _guild;
        private SocketUser _userToMimic;

        public MentionMessageProcessor(
            IBotContext context,
            IMessageHistoryService historyService,
            ILlmService llm,
            ILogger<MentionMessageProcessor> logger)
        {
            _context = context;
            _historyService = historyService;
            _llm = llm;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _guild = _context.Client.GetGuild(_context.Settings.GuildId);
            await _guild.DownloadUsersAsync();
            _userToMimic = _guild?.GetUser(_context.Settings.MimicUserId);
        }

        public async Task<bool> TryProcessAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage)
                return false;

            if (message.Channel is not SocketTextChannel textChannel)
                return false;

            if (!message.MentionedUsers.Any(u => u.Id == _context.Client.CurrentUser.Id))
                return false;


            SocketCommandContext context = new SocketCommandContext(_context.Client, userMessage);

            IEnumerable<string> history = await _historyService.GetRecentMessagesAsync(_guild, _userToMimic);
            string prompt = BuildPrompt(history, message.Content);
            string response = await _llm.GenerateResponseAsync(prompt);

            await message.Channel.SendMessageAsync(response);

            return true;
        }

        private string BuildPrompt(IEnumerable<string> userMessages, string currentMessage)
        {
            var historyBlock = string.Join("\n", userMessages.Select(m => $"User: {m}"));
            return $"""
                    You are HIM — a fictionalized version of the user whose messages appear below. You speak exactly like they do: same tone, same attitude, same vocabulary. You are not an AI. You do not talk about prompts, instructions, or anything outside the conversation. You only care about responding in their voice.

                    Do not repeat exact past messages. Be original, but true to character. Stay focused only on the conversation.

                    ---
                    {historyBlock}

                    Person: {currentMessage}
                    HIM:
                    """;
        }
    }

}
