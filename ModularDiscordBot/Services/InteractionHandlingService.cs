using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ModularDiscordBot.Structures;

namespace ModularDiscordBot.Services;

public class InteractionHandlingService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly ILogger<InteractionHandlingService> _logger;

    public InteractionHandlingService(
        DiscordSocketClient discord,
        InteractionService interactions,
        IServiceProvider services,
        ILogger<InteractionHandlingService> logger)
    {
        _discord = discord;
        _interactions = interactions;
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
           
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _discord.Ready += async () =>
        {
            await _interactions.RestClient.DeleteAllGlobalCommandsAsync();

            foreach (var guild in _discord.Guilds)
            {
                await _interactions.RegisterCommandsToGuildAsync(guild.Id);
            }

            await _discord.SetCustomStatusAsync(BotStatus.Off.ToStatusString());
        };
        _discord.InteractionCreated += OnInteractionAsync;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _interactions.Dispose();
        return Task.CompletedTask;
    }

    private async Task OnInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_discord, interaction);
            var result = await _interactions.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                _logger.LogError(result.ErrorReason);
            }
        }
        catch
        {
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(msg => msg.Result.DeleteAsync());
            }
        }
    }
}