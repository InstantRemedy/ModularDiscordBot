using Microsoft.Extensions.DependencyInjection;
using ModularDiscordBot.Configuration.Configurations;

namespace ModularDiscordBot.Configuration.IoC;

public static class ConfigurationIoC
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        return services
            .AddSingleton<BotConfiguration>(_ =>
            {
                return ConfigurationManager.Load<BotConfiguration>()!;
            })
            .AddSingleton<OpenAiConfiguration>(_ =>
            {
                return ConfigurationManager.Load<OpenAiConfiguration>()!;
            })
            .AddSingleton<RoundStatusConfiguration>(_ =>
            {
                return ConfigurationManager.Load<RoundStatusConfiguration>()!;
            });
    }
}