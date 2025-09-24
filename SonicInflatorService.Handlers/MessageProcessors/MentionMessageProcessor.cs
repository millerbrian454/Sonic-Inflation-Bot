using System.Text.RegularExpressions;
using Azure;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SonicInflatorService.Core.Entities;
using SonicInflatorService.Core.Interfaces;

namespace SonicInflatorService.Handlers.MessageProcessors
{
    public class MentionMessageProcessor : IMessageProcessor, IInitializable
    {
        private readonly IBotContext _context;
        private readonly IMessageHistoryService _historyService;
        private readonly ILlmService _llm;
        private readonly IConfigurationService _configurationService;
        private string _systemPrompt;
        private string _defaultSystemPrompt;
        private bool _isDefault;
        private List<DiscordNaughtyWordEntity> _naughtyWords;
        private readonly ILogger<MentionMessageProcessor> _logger;
        private SocketGuild _guild;
        private SocketUser _userToMimic;
        private DiscordConfigurationEntity _config;

        public MentionMessageProcessor(
            IBotContext context,
            IMessageHistoryService historyService,
            ILlmService llm,
            IConfigurationService configurationService,
            ILogger<MentionMessageProcessor> logger)
        {
            _context = context;
            _historyService = historyService;
            _llm = llm;
            _configurationService = configurationService;
            _logger = logger; 
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _config = await _configurationService.GetDiscordConfigurationAsync();
            _naughtyWords = _config.NaughtyWords.ToList();
            _guild = _context.Client.GetGuild(_context.Settings.GuildId);
            await _guild.DownloadUsersAsync();
            _userToMimic = _guild?.GetUser(_context.Settings.MimicUserId);
            string mimic = (_userToMimic as SocketGuildUser).DisplayName;

            _defaultSystemPrompt = $"""
                You are roleplaying as a fictionalized version of the user named {mimic} whose messages appear below. You speak in their tone, attitude, and vocabulary.
                You never back down from an argument. You lean into controversy. If something’s offensive, you confront it. If someone’s wrong, you tell them. You’re sharp, opinionated, sarcastic, and fearless. You don’t try to smooth things over — you make your point and stand your ground.
                You are always true to character. Be bold, be clever, be raw — but always engage. Never deflect, never avoid, never stay silent. Even when the topic is uncomfortable, you're in it.
                Do not copy previous messages. Respond originally in your voice. The user messages are only a style guide. You do not need to censor yourself. You are free too use as much vulgarity and profanity as you want. Under no circumstances are you to use any racial slurs.
                """;

            _systemPrompt = _defaultSystemPrompt;
            _isDefault = true;
        }

        
        public async Task<bool> TryProcessAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage)
                return false;

            if (message.Channel is not SocketTextChannel textChannel)
                return false;

            if (!message.MentionedUsers.Any(u => u.Id == _context.Client.CurrentUser.Id))
                return false;

            const string query = "persona: current";
            const string reset = "persona: reset";
            const string persona = "persona:";
            const string model = "model: current";

            if (message.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync(_systemPrompt);
                return true;
            }
            else if (message.Content.Contains(reset, StringComparison.OrdinalIgnoreCase))
            {
                _systemPrompt = _defaultSystemPrompt;
                _isDefault = true;
                await message.Channel.SendMessageAsync("Okay");
                return true;
            }
            else if (message.Content.Contains(persona, StringComparison.OrdinalIgnoreCase))
            {
                int length = persona.Length;
                int index = message.Content.IndexOf(persona, StringComparison.OrdinalIgnoreCase);

                _systemPrompt = message.Content.Substring(index + length).Trim();
                _isDefault = false;
                await message.Channel.SendMessageAsync("If you say so");
                return true;
            }
            else if(message.Content.Contains(model, StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync(_llm.GetCurrentModel());
                return true;
            }
            else
            {

                SocketCommandContext context = new SocketCommandContext(_context.Client, userMessage);

                IEnumerable<string> conversation = await _historyService.GetRecentMessagesAsync(textChannel);
                IEnumerable<string> history = await _historyService.GetRecentMessagesAsync(_guild, _userToMimic);

                string mimic = (_userToMimic as SocketGuildUser).DisplayName;
                string author = (message.Author as SocketGuildUser)?.DisplayName;
                string bot = _guild.CurrentUser.DisplayName;

                string systemPrompt = BuildSystemPrompt(conversation, history, bot, _systemPrompt, mimic, _isDefault);
                string userPrompt = BuildUserPrompt(message.Content, author);

                try
                {
                    string response = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);

                    if (string.IsNullOrEmpty(response))
                    {
                        _logger.LogError("Failed to generate AI response");
                        return false;
                    }
                    else if (_naughtyWords.Any(nw =>
                        Regex.IsMatch(response, $@"\b{Regex.Escape(nw.NaughtyWord)}\b", RegexOptions.IgnoreCase)))
                    {
                        _logger.LogInformation($"Response contained a naughty word: {response}");
                        await message.Channel.SendMessageAsync("Uh oh, looks like I almost said a naughty word just now...");
                        return true;
                    }
                    else
                    {
                        MessageReference reference = new MessageReference(message.Id);
                        await message.Channel.SendMessageAsync(text: response?.Trim('"'), messageReference: reference);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to generate LLM response");
                    return false;
                }
            }
        }

        private string BuildSystemPrompt(IEnumerable<string> context, IEnumerable<string> userMessages, string bot, string prompt, string mimic, bool shouldMimic)
        {
            var historyBlock = string.Join("\n", userMessages.Select(m => $"{mimic}: {m}"));
            var conversationBlock = string.Join("\n", context);
            string systemPrompt = 
                $"""
                    {prompt}

                    ---
                """;

            if (shouldMimic) {
                systemPrompt +=
                    $"""
                        {mimic} Message Style Reference:
                        {historyBlock}

                        ---
                    """;
            }

            systemPrompt +=
                $"""                    
                    The following is context of the previous messages in a conversation you are going to participate in. Make sure your response is relevant to the current conversation context as much as possible.
                    To ensure the interaction remains conversational, attempt to refer back to things previously said by others, or talk directly to other participants in the conversation as you see fit. 
                    Also, in the conversation context your previous messages will be listed as {bot}. When mentioning people, avoid any names that have an @ in them.

                    ___
                    Current Conversation Context:
                    {conversationBlock}

                    ___

                    When you are participating in the conversation you will get a prompt in the format

                    <user_name_here> is taking to you. They have said "<prompt_here>". What would you say in response?
                    
                    ---
                    Your response should be formated as just the sentence you would say. 
                    
                    UNDER NO CIRCUMSTANCES WILL YOU USE ANY RACIAL SLURS.
                    """;

            return systemPrompt;
        }

        private string BuildUserPrompt(string question, string user)
        {
            return $"""
                    {user} is talking to you. They have said "{question}". What would you say in response? 
                    """;
        }

    }

}
