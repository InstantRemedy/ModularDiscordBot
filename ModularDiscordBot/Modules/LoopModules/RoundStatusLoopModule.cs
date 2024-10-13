using Discord.Commands;
using Discord.WebSocket;
using ModularDiscordBot.Attributes;
using ModularDiscordBot.Controllers;

namespace ModularDiscordBot.Modules.LoopModules;

[Name("round-status")]
public class RoundStatusLoopModule : LoopModule
{
    private readonly RoundStatusController _controller;
    
    public RoundStatusLoopModule(
        DiscordSocketClient client,
        RoundStatusController controller) 
        : base(client)
    {
        _controller = controller;
    }
    
    [Loop(0, 0, 10)]
    public async Task RoundStatusLoopAsync()
    {
        await _controller.CheckStatusAsync();
    }
}