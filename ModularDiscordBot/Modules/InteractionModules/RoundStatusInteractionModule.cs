using System.Net;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration;
using ModularDiscordBot.Configuration.Configurations;
using ContextType = Discord.Commands.ContextType;

namespace ModularDiscordBot.Modules.InteractionModules;

[Name("round-status")]
public sealed class RoundStatusInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RoundStatusInteractionModule> _logger;
    private readonly RoundStatusConfiguration _configuration;
    
    RoundStatusInteractionModule(
        ILogger<RoundStatusInteractionModule> logger,
        RoundStatusConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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
    
    [SlashCommand("rs_host", "Set the host for the round status")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task SetHostAsync(string host)
    {
        try
        {
            await DeferAsync(ephemeral: true);
            
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral: true);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(host))
            {
                await FollowupAsync("Invalid host", ephemeral: true);
                return;
            }

            if (!IPAddress.TryParse(host, out _))
            {
                await FollowupAsync("Invalid host", ephemeral: true);
                return;
            }
        
            _configuration.Host = host;
            ConfigurationManager.Save(_configuration);
            
            await FollowupAsync($"Set host to {host}", ephemeral: true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting host");
            await FollowupAsync("Error setting host", ephemeral: true);
        }
    }
    
    [SlashCommand("rs_port", "Set the port for the round status")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task SetPortAsync(int port)
    {
        await DeferAsync(ephemeral: true);
        
        try
        {
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral:true);
                return;
            }
            
            if (port < 1 || port > 65535)
            {
                await FollowupAsync("Invalid port", ephemeral:true);
                return;
            }
        
            _configuration.Port = port;
            ConfigurationManager.Save(_configuration);
            
            await FollowupAsync($"Set port to {port}", ephemeral:true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting port");
            await FollowupAsync("Error setting port", ephemeral:true);
        }
    }
    
    [SlashCommand("rs_add_allowed_role", "Add a role that can access the round status")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task AddRoleAsync(SocketRole? role)
    {
        await DeferAsync(ephemeral:true);
        
        try
        {
            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral:true);
                return;
            }

            if (role == null)
            {
                await FollowupAsync("Invalid role", ephemeral:true);
                return;
            }
            
            if (_configuration.AllowedRoleIds.Contains(role.Id))
            {
                await FollowupAsync("Role already added", ephemeral:true);
                return;
            }
        
            if (role.Id == _configuration.MainRoleId)
            {
                await FollowupAsync("You cannot add the main role", ephemeral:true);
                return;
            }

            _configuration.AllowedRoleIds.Add(role.Id);
            ConfigurationManager.Save(_configuration);
            
            await FollowupAsync($"Added role {role.Name}", ephemeral:true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding role");
            await FollowupAsync("Error adding role", ephemeral:true);
        }
    }
    
    [SlashCommand("rs_remove_allowed_role", "Remove a role that can access the round status")]
    [Discord.Commands.RequireContext(ContextType.Guild)]
    [Discord.Commands.RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task RemoveRoleAsync(SocketRole? role)
    {
        try
        {
            await DeferAsync(ephemeral:true);

            if (!IsRoleAccess())
            {
                await FollowupAsync("You do not have access to this command", ephemeral:true);
                return;
            }
            
            if (role == null)
            {
                await FollowupAsync("Invalid role", ephemeral:true);
                return;
            }
            
            if (!_configuration.AllowedRoleIds.Contains(role.Id))
            {
                await FollowupAsync("Role not found in configuration", ephemeral:true);
                return;
            }
            if (role.Id == _configuration.MainRoleId)
            {
                await FollowupAsync("You cannot remove the main role", ephemeral:true);
                return;
            }

            _configuration.AllowedRoleIds.Remove(role.Id);
            ConfigurationManager.Save(_configuration);
            
            await FollowupAsync($"Removed role {role.Name}", ephemeral:true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing role");
            await FollowupAsync("Error removing role", ephemeral:true);
        }
    }
    
}