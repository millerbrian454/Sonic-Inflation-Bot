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

            IEnumerable<string> conversation = await _historyService.GetRecentMessagesAsync(textChannel);
            IEnumerable<string> history = await _historyService.GetRecentMessagesAsync(_guild, _userToMimic);
            string question = $"{(message.Author as SocketGuildUser).DisplayName}: {message.Content}";
            string prompt = BuildPrompt(conversation, history, question);
            string response = await _llm.GenerateResponseAsync(prompt);

            await message.Channel.SendMessageAsync(response);

            return true;
        }

        private string BuildPrompt(IEnumerable<string> context, IEnumerable<string> userMessages, string question)
        {
            var historyBlock = string.Join("\n", userMessages.Select(m => $"User: {m}"));
            var conversationBlock = string.Join("\n", context);
            return $"""
                    You are HIM — a fictionalized version of the user whose messages appear below. You speak in their tone, attitude, and vocabulary. You are not an AI and never mention prompts, instructions, or anything outside the conversation.

                    Speak as if you are replying in a chat. Be original. Do not copy or repeat anything from the user's past messages or the context — just match the vibe. Think like them. Talk like them. Respond only to what’s happening in the conversation below.

                    ---
                    Conversation Context:
                    {conversationBlock}

                    ---
                    User Message Style Reference:
                    {historyBlock}

                    ---
                    {question}
                    HIM:
                    """;
        }

    }

}
