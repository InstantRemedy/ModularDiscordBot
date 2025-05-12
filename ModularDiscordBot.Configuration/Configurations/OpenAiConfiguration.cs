using System.Text.Json.Serialization;
using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration.Configurations;

[Configuration("open-ai")]
public sealed class OpenAiConfiguration
{
    [JsonPropertyName("main_role_id")]
    public ulong MainRoleId { get; set; } = 0;
    
    [JsonPropertyName("allowed_role_ids")]
    public HashSet<ulong> AllowedRoleIds { get; set; } = new();
    
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; } = string.Empty;
    
    [JsonPropertyName("assistant_id")]
    public string AssistantId { get; set; } = string.Empty;
    
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; set; } = string.Empty;
    
    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; } = string.Empty;
    
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; set; } = string.Empty;
    
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = string.Empty;
}