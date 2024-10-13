using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration.Configurations;

[Configuration("round-status")]
public sealed class RoundStatusConfiguration : Configuration
{
    [PropertyConfiguration("host")]
    public string Host { get; set; } = string.Empty;
    
    [PropertyConfiguration("port")]
    public int Port { get; set; } = 0;
    
    [PropertyConfiguration("channel_id")]
    public ulong ChannelId { get; set; } = 0;
    
    [PropertyConfiguration("main_role_id")]
    public ulong MainRoleId { get; set; } = 0;
    
    [PropertyConfiguration("allowed_role_ids")]
    public HashSet<ulong> AllowedRoleIds { get; set; } = new();
}