using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Interfaces;
using Newtonsoft.Json.Linq;
using OpenAI;

namespace ModularDiscordBot.Controllers;

public sealed class OpenAiController : IBotController
{
    private readonly OpenAIClient _openAiClient;
    private readonly OpenAiConfiguration _openAiConfiguration;
    private readonly ILogger<OpenAiController> _logger;
    
    private IUserMessage? _currentMessage;

    public OpenAiController(
        OpenAIClient openAiClient,
        OpenAiConfiguration openAiConfiguration,
        ILogger<OpenAiController> logger)
    {
        _openAiClient = openAiClient;
        _openAiConfiguration = openAiConfiguration;
        _logger = logger;
    }
    
    public async Task MindAsync(string prompt, SocketCommandContext context)
    {
        await context.Channel.TriggerTypingAsync();
        if (_openAiConfiguration.Mode == "stream")
        {
            await MindStreamAsync(prompt, context);
            return;
        }
        
        if (_openAiConfiguration.Mode != "no_stream")
        {
            _openAiConfiguration.Mode = "no_stream";
            await _openAiConfiguration.SaveConfigurationAsync();
        }
            
        await MindNoStreamAsync(prompt, context);
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
    
    #region MindMessage
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
}