using System.Text;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Interfaces;
using ModularDiscordBot.Structures;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Threads;

namespace ModularDiscordBot.Controllers;

public sealed class OpenAiController : IBotController
{
    public uint MaxRequests { get; set; }
    public uint RequestAmount { get; set; }
    public bool IsEnabled { get; set; }
    
    private string ExhaustedMessage =>
        new StringBuilder()
            .Append("Увы, мои силы иссякли, и мне нужно время на восстановление. ")
            .Append("Скоро я буду готова к новой беседе.")
            .ToString();
    
    private string UnavailableMessage =>
            new StringBuilder()
                .Append("Простите, но я не могу ответить на ваш вопрос. ")
                .Append("Попробуйте позже")
                .ToString();
    
    private readonly OpenAIClient _openAiClient;
    private readonly OpenAiConfiguration _openAiConfiguration;
    
    private IUserMessage? _currentMessage;

    public OpenAiController(
        OpenAIClient openAiClient,
        OpenAiConfiguration openAiConfiguration)
    {
        _openAiClient = openAiClient;
        _openAiConfiguration = openAiConfiguration;
    }
    
    public async Task MindAsync(string prompt, SocketCommandContext context)
    {
        if (!IsEnabled || RequestAmount >= MaxRequests)
        {
            await context.Message.ReplyAsync(ExhaustedMessage);
            return;
        }
        
        await context.Channel.TriggerTypingAsync();
        
        if (string.IsNullOrWhiteSpace(prompt))
        {
            var assistant = await _openAiClient.AssistantsEndpoint.RetrieveAssistantAsync(_openAiConfiguration.AssistantId);
            var text = $"Привет! я {assistant.Name}. Чего бы {context.User.Username} хотел знать?";
            await context.Message.ReplyAsync(text);
            return;
        }
        
        switch (StreamModeHelper.FromString(_openAiConfiguration.Mode))
        {
            case StreamMode.Stream:
            {
                try
                {
                    await MindStreamAsync(prompt, context);
                }
                catch (Exception)
                {
                    await context.Message.ReplyAsync(UnavailableMessage);
                }
                break;
            }
            case StreamMode.NoStream:
            {
                try
                {
                    await MindNoStreamAsync(prompt, context);
                }
                catch (Exception)
                {
                    await context.Message.ReplyAsync(UnavailableMessage);
                }
                break;
            }
            default:
            {
                try
                {
                    await MindStreamAsync(prompt, context);
                }
                catch (Exception)
                {
                    await context.Message.ReplyAsync(UnavailableMessage);
                }
                break;
            }
        }
        
        RequestAmount++;
    }
    
    #region MindStream
    private async Task MindStreamAsync(string prompt, SocketCommandContext context)
    {
        var formattedPrompt = $"{context.User.Username}:{prompt}";
        _ = await _openAiClient.ThreadsEndpoint.CreateMessageAsync(
            threadId: _openAiConfiguration.ThreadId,
            message: new OpenAI.Threads.Message(formattedPrompt));

        _ = await _openAiClient.ThreadsEndpoint.CreateRunAsync(
            threadId: _openAiConfiguration.ThreadId,
            request: new OpenAI.Threads.CreateRunRequest(assistantId: _openAiConfiguration.AssistantId),
            streamEventHandler: async (sentEvent) => await HandleStreamResponse(sentEvent, context),
            cancellationToken: new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
        
        _currentMessage = null;
    }
    
    private async Task HandleStreamResponse(IServerSentEvent sentEvent, SocketCommandContext context)
    {
        var jObject = JObject.Parse(sentEvent.ToJsonString());
        var objectType = jObject.Value<string>("object");

        if (objectType != "thread.message.delta")
        {
            return;
        }
            
        var deltaObject = jObject["delta"];
        var roleInt = deltaObject!.Value<int>("role");
        var role = Enum.Parse<Role>(roleInt.ToString());
            
        if (role != Role.Assistant)
        {
            return;
        }
            
        var contentObject = deltaObject["content"]![0];
        var valueType = contentObject!.Value<string>("type");
        
        if(valueType != "text")
        {
            return;
        }
        
        var deltaText = contentObject["text"]!.Value<string>("value"); 
            
        if (_currentMessage == null)
        {
            _currentMessage = await context.Message.ReplyAsync(deltaText);
        }
        else
        {
            var fullMessage = _currentMessage.Content + deltaText;
            await _currentMessage!.ModifyAsync(x => x.Content = fullMessage);
        }
    }
    #endregion
    
    #region MindNoSteam
    private async Task MindNoStreamAsync(string prompt, SocketCommandContext context)
    {
        var formattedPrompt = $"{context.User.Username}:{prompt}";
        _ = await _openAiClient.ThreadsEndpoint.CreateMessageAsync(
            threadId: _openAiConfiguration.ThreadId,
            message: new OpenAI.Threads.Message(formattedPrompt));

        var messageId = string.Empty;
        _ = await _openAiClient.ThreadsEndpoint.CreateRunAsync(
            threadId: _openAiConfiguration.ThreadId,
            request: new OpenAI.Threads.CreateRunRequest(assistantId: _openAiConfiguration.AssistantId),
            streamEventHandler: serverSent =>
            {
                if(string.IsNullOrWhiteSpace(messageId))
                {
                    messageId = GetMessageId(serverSent);
                }
                return Task.CompletedTask;
            },
            cancellationToken: new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
        
        var message = 
            await _openAiClient.ThreadsEndpoint.RetrieveMessageAsync(
                threadId: _openAiConfiguration.ThreadId, 
                messageId: messageId);

        var textContent = message.Content[0].Text as TextContent;
        var textValue = textContent!.Value;

        await context.Message.ReplyAsync(textValue);
    }


    private string GetMessageId(IServerSentEvent sentEvent)
    {
        var jObject = JObject.Parse(sentEvent.ToJsonString());
        var objectType = jObject.Value<string>("object");
        
        if (objectType != "thread.message")
        {
            return string.Empty;
        }
        
        return jObject.Value<string>("id")!;
    }
    #endregion

    #region Interaction

    public async Task CreateNewThreadAsync(SocketInteractionContext context)
    {
        await _openAiClient.ThreadsEndpoint.DeleteThreadAsync(_openAiConfiguration.ThreadId);
        var thread = await _openAiClient.ThreadsEndpoint.CreateThreadAsync();
        _openAiConfiguration.ThreadId = thread.Id;
        await _openAiConfiguration.SaveConfigurationAsync();
    }
    
    #endregion
}