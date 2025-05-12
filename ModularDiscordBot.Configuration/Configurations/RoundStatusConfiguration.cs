using System.Text.Json.Serialization;
using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration.Configurations;

[Configuration("round-status")]
public sealed class RoundStatusConfiguration
{
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;
    
    [JsonPropertyName("port")]
    public int Port { get; set; } = 0;
    
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; set; } = 0;
    
    [JsonPropertyName("main_role_id")]
    public ulong MainRoleId { get; set; } = 0;
    
    [JsonPropertyName("allowed_role_ids")]
    public HashSet<ulong> AllowedRoleIds { get; set; } = new();
}