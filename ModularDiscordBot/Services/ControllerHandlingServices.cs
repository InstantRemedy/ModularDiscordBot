using ModularDiscordBot.Controllers;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularDiscordBot.Services;

internal sealed class ControllerHandlingService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;
    private readonly ILogger<ControllerHandlingService> _logger;
    
    public ControllerHandlingService(
        IServiceProvider services)
    {
        _services = services;
        
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _logger = services.GetRequiredService<ILogger<ControllerHandlingService>>();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var controllers = _services.GetServices<ControllerBase>();
        
        _discord.Ready += async () =>
        {
            foreach (var controller in controllers)
            {
                try
                {
                    await controller.OnInitialized();
                    controller.IsInitialized = true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to initialize controller {controller.GetType().Name}");
                }

            }
        };
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var controllers = _services.GetServices<ControllerBase>();
        
        foreach (var controller in controllers)
        {
            await controller.OnShutdown();
            controller.IsInitialized = false;
        }
    }
}