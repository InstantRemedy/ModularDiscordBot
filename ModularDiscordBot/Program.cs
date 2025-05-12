using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Configuration.IoC;
using ModularDiscordBot.Ioc;
using ModularDiscordBot.Services;
using OpenAI;
using RunMode = Discord.Commands.RunMode;

namespace ModularDiscordBot;

/// <summary>
/// The entry point of the bot.
/// </summary>
internal sealed class Program
{
    private static async Task Main()
    {
        var builder = new HostBuilder()
            .ConfigureLogging(x =>
            {
                x.AddConsole();
                x.AddFile("logs/{Date}.txt");
                x.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices((_, services) =>
            {
                services
                    .AddHostedService<CommandHandlingService>()
                    .AddHostedService<InteractionHandlingService>()
                    .AddHostedService<LoopHandlingService>()
                    .AddHostedService<ControllerHandlingService>()
                    .AddConfiguration()
                    .AddControllers()
                    .AddHttpClient()
                    .AddSingleton<OpenAIClient>(provider =>
                    {
                        var openAiConfig = provider.GetRequiredService<OpenAiConfiguration>();
                        return new OpenAIClient(new OpenAIAuthentication(
                            apiKey: openAiConfig.ApiKey,
                            organization: openAiConfig.OrganizationId,
                            projectId: openAiConfig.ProjectId));
                    });
                    
                services.AddDiscordHost((config, provider) =>
                {
                    var botConfig = provider.GetRequiredService<BotConfiguration>();
                    
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Debug,
                        AlwaysDownloadUsers = false,
                        MessageCacheSize = 200,
                        GatewayIntents = GatewayIntents.All
                    };
                    config.Token = botConfig.Token;
                });

                services.AddCommandService((config, _) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Sync;
                });
                
                services.AddInteractionService((config, _) =>
                {
                    config.LogLevel = LogSeverity.Debug;
                });
            })
            .UseConsoleLifetime();

        var host = builder.Build();
        using (host)
        {
            await host.RunAsync();
        }
    }
}