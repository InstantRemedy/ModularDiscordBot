using System.Collections.Concurrent;
using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;
using IResult = Discord.Commands.IResult;

namespace ModularDiscordBot.Services;

public class CommandHandlingService : ModularDiscordBotService
{
    private readonly IServiceProvider _provider;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _service;
    private readonly ConcurrentQueue<SocketMessage> _messageQueue = new();
    private bool _isProcessingQueue = false;
    
    public CommandHandlingService(
        IServiceProvider provider, 
        DiscordSocketClient client, 
        CommandService service,
        BotConfiguration botConfiguration, 
        ILogger<DiscordClientService> logger)
        : base(client: client, 
            logger: logger, 
            botConfiguration: botConfiguration)
    {
        _provider = provider;
        _client = client;
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived += OnMessageReceived;
        _service.CommandExecuted += OnCommandExecuted;
        await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
    }

    private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
    {
        if (result.IsSuccess)
        {
            return;
        }
        
        await commandContext.Channel.SendMessageAsync(result.ErrorReason);
    }

    private Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (!(socketMessage is SocketUserMessage message))
        {
            return Task.CompletedTask;
        }

        if (message.Source != MessageSource.User)
        {
            return Task.CompletedTask;
        }

        // Добавляем сообщение в очередь
        _messageQueue.Enqueue(socketMessage);

        // Начинаем обработку очереди, если она еще не запущена
        if (!_isProcessingQueue)
        {
            _isProcessingQueue = true;
            _ = ProcessMessageQueue();
        }
        
        return Task.CompletedTask;
    }

    private async Task ProcessMessageQueue()
    {
        while (_messageQueue.TryDequeue(out var socketMessage))
        {
            if (socketMessage is not SocketUserMessage message)
            {
                continue;
            }

            var argPos = 0;
            var user = message.Author as SocketGuildUser;
            var prefix = BotConfiguration.CommandPrefix;
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                continue;
            }

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        _isProcessingQueue = false;
    }
}
