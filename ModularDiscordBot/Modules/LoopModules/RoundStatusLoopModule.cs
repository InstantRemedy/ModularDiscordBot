using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Attributes;
using ModularDiscordBot.Controllers;

namespace ModularDiscordBot.Modules.LoopModules;

[Name("round-status")]
public class RoundStatusLoopModule : LoopModule
{
    private readonly RoundStatusController _controller;
    private readonly ILogger<RoundStatusLoopModule> _logger;
    
    public RoundStatusLoopModule(
        DiscordSocketClient client,
        RoundStatusController controller,
        ILogger<RoundStatusLoopModule> logger) 
        : base(client)
    {
        _controller = controller;
        _logger = logger;    
    }
    
    [Loop(0, 0, 10)]
    public async Task RoundStatusLoopAsync()
    {
        await _controller.CheckStatusAsync();
    }
}