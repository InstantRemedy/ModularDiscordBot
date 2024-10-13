using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Controllers;
using ContextType = Discord.Commands.ContextType;

namespace ModularDiscordBot.Modules.InteractionModules;

public sealed class OpenAiInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly OpenAiController _openAiController;
    private readonly OpenAiConfiguration _openAiConfiguration;
    private readonly ILogger<OpenAiInteractionModule> _logger;
    
    public OpenAiInteractionModule(
        OpenAiController openAiController,
        OpenAiConfiguration openAiConfiguration,
        ILogger<OpenAiInteractionModule> logger)
    {
        _openAiController = openAiController;
        _openAiConfiguration = openAiConfiguration;
        _logger = logger;
    }
    
    private bool IsRoleAccess()
    {
        var user = Context.User as SocketGuildUser;
        
        if (user is null)
        {
            return false;
        }
        
        return user.Roles.Any(x => x.Id == _openAiConfiguration.MainRoleId) ||
               user.Roles.Any(x => _openAiConfiguration.AllowedRoleIds.Contains(x.Id));
    }
    
    [SlashCommand("openai_mode", "Set the mode for the OpenAI(steam/no_stream)")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task SetModeAsync(string mode)
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(mode))
            {
                await FollowupAsync("Invalid mode", ephemeral: true);
                return;
            }

            if (mode != "stream" && mode != "no_stream")
            {
                await FollowupAsync("Invalid mode", ephemeral: true);
                return;
            }
            
            _openAiConfiguration.Mode = mode;
            await _openAiConfiguration.SaveConfigurationAsync();
            
            await FollowupAsync($"Mode set: {mode}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting mode");
            await FollowupAsync("Error setting mode", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_add_allowed_role", "Add a role to the allowed roles")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task AddAllowedRoleAsync(SocketRole? role)
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            if (role is null)
            {
                await FollowupAsync("Invalid role", ephemeral: true);
                return;
            }
            
            if (_openAiConfiguration.AllowedRoleIds.Contains(role.Id))
            {
                await FollowupAsync("Role already added", ephemeral: true);
                return;
            }
            
            if (role.Id == _openAiConfiguration.MainRoleId)
            {
                await FollowupAsync("Role is the main role", ephemeral: true);
                return;
            }
            
            _openAiConfiguration.AllowedRoleIds.Add(role.Id);
            await _openAiConfiguration.SaveConfigurationAsync();
            
            await FollowupAsync($"Role added: {role.Name}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding role");
            await FollowupAsync("Error adding role", ephemeral: true);
        }
    }
}