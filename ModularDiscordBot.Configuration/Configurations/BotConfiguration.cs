using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration.Configurations;

[Configuration("bot")]
public sealed class BotConfiguration : Configuration
{
    [PropertyConfiguration("token")]
    public string Token { get; set; } = string.Empty;
    
    [PropertyConfiguration("command_prefix")]
    public string CommandPrefix { get; set; } = "!";
}