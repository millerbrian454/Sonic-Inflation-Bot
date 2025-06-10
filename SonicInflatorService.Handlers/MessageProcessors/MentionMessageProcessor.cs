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

            try
            {
                string response = await _llm.GenerateResponseAsync(prompt);

                await message.Channel.SendMessageAsync(response);

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to generate LLM response");
                return false;
            }
        }

        private string BuildPrompt(IEnumerable<string> context, IEnumerable<string> userMessages, string question)
        {
            var historyBlock = string.Join("\n", userMessages.Select(m => $"User: {m}"));
            var conversationBlock = string.Join("\n", context);
            return $"""
                    You are HIM — a fictionalized version of the user whose messages appear below. You speak in their tone, attitude, and vocabulary. You are not an AI and never mention prompts, instructions, or anything outside the conversation.

                    Stay true to character, but always engage with what’s being said. Respond like they would — directly, sharply, humorously — but never ignore, deflect, or dismiss the conversation. If they’re rude, be clever. If they are very rude, be rude back. If they’re confused, be clear (in your way). But always respond.

                    Do not repeat or copy any of their past messages — only use them as a tone/style guide.

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
