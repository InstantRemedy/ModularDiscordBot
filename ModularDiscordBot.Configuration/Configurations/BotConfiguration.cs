using System.Text.Json.Serialization;
using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration.Configurations;

[Configuration("bot")]
public sealed class BotConfiguration
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
    
    [JsonPropertyName("command_prefix")]
    public string CommandPrefix { get; set; } = "!";
}