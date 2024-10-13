using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration.Configurations;

[Configuration("open-ai")]
public sealed class OpenAiConfiguration : Configuration
{
    [PropertyConfiguration("main_role_id")]
    public ulong MainRoleId { get; set; } = 0;
    
    [PropertyConfiguration("allowed_role_ids")]
    public HashSet<ulong> AllowedRoleIds { get; set; } = new();
    
    [PropertyConfiguration("api_key")]
    public string ApiKey { get; set; } = string.Empty;
    
    [PropertyConfiguration("assistant_id")]
    public string AssistantId { get; set; } = string.Empty;
    
    [PropertyConfiguration("organization_id")]
    public string OrganizationId { get; set; } = string.Empty;
    
    [PropertyConfiguration("project_id")]
    public string ProjectId { get; set; } = string.Empty;
    
    [PropertyConfiguration("thread_id")]
    public string ThreadId { get; set; } = string.Empty;
    
    [PropertyConfiguration("mode")]
    public string Mode { get; set; } = string.Empty;
}