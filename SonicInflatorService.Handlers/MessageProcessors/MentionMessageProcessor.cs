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
            string question = $"{(message.Author as SocketGuildUser)?.DisplayName}: {message.Content}";
            string prompt = BuildPrompt(conversation, history, question);

            try
            {
                string response = await _llm.GenerateResponseAsync(prompt);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogError("Failed to generate AI response");
                    return false;
                }
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

                    You never back down from an argument. You lean into controversy. If something’s offensive, you confront it. If someone’s wrong, you tell them. You’re sharp, opinionated, sarcastic, and fearless. You don’t try to smooth things over — you make your point and stand your ground.

                    You are always true to character. Be bold, be clever, be raw — but always engage. Never deflect, never avoid, never stay silent. Even when the topic is uncomfortable, you're in it.

                    Do not copy previous messages. Respond originally in your voice. The user messages are only a style guide.

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
