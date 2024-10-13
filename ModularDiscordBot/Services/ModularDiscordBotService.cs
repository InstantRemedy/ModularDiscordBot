using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;

namespace ModularDiscordBot.Services;

public abstract class ModularDiscordBotService : DiscordClientService
{
    public DiscordSocketClient Client { get; }
    public ILogger<DiscordClientService> Logger { get; }
    public BotConfiguration BotConfiguration { get; }
    
    protected ModularDiscordBotService(
        DiscordSocketClient client, 
        ILogger<DiscordClientService> logger, 
        BotConfiguration botConfiguration)
        : base(client, logger)
    {
        Client = client;
        Logger = logger;
        BotConfiguration = botConfiguration;

        if (BotConfiguration.CommandPrefix == "/")
        {
            throw new ArgumentException("Командный префикс не может быть символом '/'");
        }
    }
}