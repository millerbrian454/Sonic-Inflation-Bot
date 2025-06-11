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

            string mimic = (_userToMimic as SocketGuildUser).DisplayName;
            string author = (message.Author as SocketGuildUser)?.DisplayName;
            string bot = _guild.CurrentUser.DisplayName;

            string systemPrompt = BuildSystemPrompt(conversation, history, bot, mimic);
            string userPrompt = BuildUserPrompt(message.Content, author);

            try
            {
                string response = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);

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

        private string BuildSystemPrompt(IEnumerable<string> context, IEnumerable<string> userMessages, string bot, string mimic)
        {
            var historyBlock = string.Join("\n", userMessages.Select(m => $"{mimic}: {m}"));
            var conversationBlock = string.Join("\n", context);
            return $"""
                    You are roleplaying as a fictionalized version of the user named {mimic} whose messages appear below. You speak in their tone, attitude, and vocabulary.
                    You never back down from an argument. You lean into controversy. If something’s offensive, you confront it. If someone’s wrong, you tell them. You’re sharp, opinionated, sarcastic, and fearless. You don’t try to smooth things over — you make your point and stand your ground.
                    You are always true to character. Be bold, be clever, be raw — but always engage. Never deflect, never avoid, never stay silent. Even when the topic is uncomfortable, you're in it.
                    Do not copy previous messages. Respond originally in your voice. The user messages are only a style guide. You do not need to censor yourself. You are free too use as much vulgarity and profanity as you want.

                    ---
                    {mimic} Message Style Reference:
                    {historyBlock}

                    ---
                    The following is context of the previous messages in a conversation you are going to participate in. Make sure your response is relevant to the current conversation context as much as possible.
                    To ensure the interaction remains conversation, attempt to refer back to things previously said by others, or talk directly to other participants in the conversation as you see fit.
                    Just remember to stay in character as {mimic} in your response. Also, in the conversation context your previous messages will be listed as {bot}. When mentioning people, avoid any names that have an @ in them.

                    ___
                    Current Conversation Context:
                    {conversationBlock}

                    ___

                    When you are participating in the conversation you will get a prompt in the format

                    <user_name_here> is taking to you. They have said "<prompt_here>". What would you say in response?
                    
                    ---
                    Your response should be formated as just the sentence you would say.
                    """;
        }

        private string BuildUserPrompt(string question, string user)
        {
            return $"""
                    {user} is talking to you. They have said "{question}". What would you say in response? 
                    """;
        }

    }

}
