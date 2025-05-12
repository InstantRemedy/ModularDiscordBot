using Discord.WebSocket;

namespace ModularDiscordBot.Controllers;

public abstract class ControllerBase
{
    public bool IsInitialized { set; get; }
    
    protected readonly DiscordSocketClient Client;
    
    public ControllerBase(
        DiscordSocketClient client)
    {
        Client = client;
    }
    
    public abstract Task OnInitialized();
    public abstract Task OnShutdown();
}