using Discord.WebSocket;

namespace ModularDiscordBot.Modules;

public abstract class LoopModule
{
    protected readonly DiscordSocketClient Client;

    protected LoopModule(DiscordSocketClient client)
    {
        Client = client;
    }
}