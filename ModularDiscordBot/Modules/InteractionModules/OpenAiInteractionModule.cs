using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Controllers;
using ModularDiscordBot.Structures;
using ContextType = Discord.Commands.ContextType;

namespace ModularDiscordBot.Modules.InteractionModules;

public sealed class OpenAiInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly OpenAiController _openAiController;
    private readonly ILogger<OpenAiInteractionModule> _logger;
    private readonly OpenAiConfiguration _configuration;
    
    public OpenAiInteractionModule(
        DiscordSocketClient client,
        OpenAiController openAiController,
        OpenAiConfiguration configuration,
        ILogger<OpenAiInteractionModule> logger)
    {
        _openAiController = openAiController;
        _configuration = configuration;
        _logger = logger;
    }
    
    private bool IsRoleAccess()
    {
        var user = Context.User as SocketGuildUser;
        
        if (user is null)
        {
            return false;
        }
        
        
        return user.Roles.Any(x => x.Id == _configuration.MainRoleId) ||
               user.Roles.Any(x => _configuration.AllowedRoleIds.Contains(x.Id));
    }
    
    [SlashCommand("openai_mode", "Set the mode for the OpenAI(steam/no_stream)")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task SetModeAsync(StreamMode mode)
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            _configuration.Mode = mode.ToStreamString();
            ConfigurationManager.Save(_configuration);
            
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
            
            if (_configuration.AllowedRoleIds.Contains(role.Id))
            {
                await FollowupAsync("Role already added", ephemeral: true);
                return;
            }
            
            if (role.Id == _configuration.MainRoleId)
            {
                await FollowupAsync("Role is the main role", ephemeral: true);
                return;
            }
            
            _configuration.AllowedRoleIds.Add(role.Id);
            ConfigurationManager.Save(_configuration);
            
            await FollowupAsync($"Role added: {role.Name}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding role");
            await FollowupAsync("Error adding role", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_remove_allowed_role", "remove a role to the allowed roles")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task RemoveAllowedRoleAsync(SocketRole? role)
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
            
            if (!_configuration.AllowedRoleIds.Contains(role.Id))
            {
                await FollowupAsync("Role not contains", ephemeral: true);
                return;
            }
            
            if (role.Id == _configuration.MainRoleId)
            {
                await FollowupAsync("Role is the main role", ephemeral: true);
                return;
            }
            
            _configuration.AllowedRoleIds.Remove(role.Id);
            ConfigurationManager.Save(_configuration);
            
            await FollowupAsync($"Role removed: {role.Name}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role");
            await FollowupAsync("Error removing role", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_max_requests", "Set the max requests for the OpenAI")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task SetMaxRequestsAsync(uint maxRequests)
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            await _openAiController.SetMaxRequests(maxRequests);
            
            await FollowupAsync($"Max requests set: {maxRequests}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting max requests");
            await FollowupAsync("Error setting max requests", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_reset_requests", "Reset the request amount for the OpenAI")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task ResetRequestsAsync()
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            await _openAiController.ResetRequests();
            await FollowupAsync("Requests reset", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting requests");
            await FollowupAsync("Error resetting requests", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_enable", "Enable the OpenAI")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task EnableAsync()
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            await _openAiController.Enbale();
            
            await FollowupAsync("OpenAI enabled", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling OpenAI");
            await FollowupAsync("Error enabling OpenAI", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_disable", "Disable the OpenAI")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task DisableAsync()
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            await _openAiController.Disable();
            
            await FollowupAsync("OpenAI disabled", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling OpenAI");
            await FollowupAsync("Error disabling OpenAI", ephemeral: true);
        }
    }
    
    [SlashCommand("openai_new_thread", "Reassign the OpenAI thread")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task NewThreadAsync()
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }

            await _openAiController.CreateNewThreadAsync(Context);
            await FollowupAsync("New thread assigned", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning new thread");
            await FollowupAsync("Error assigning new thread", ephemeral: true);
        }
    }
}