using Discord.Commands;
using ModularDiscordBot.Controllers;

namespace ModularDiscordBot.Modules.CommandModules;

public sealed class OpenAiCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly OpenAiController _openAiController;
    
    public OpenAiCommandModule(OpenAiController openAiController)
    {
        _openAiController = openAiController;
    }
    
    [Command("mind")]
    private async Task Mind([Remainder] string prompt = "")
    {
        await _openAiController.MindAsync(prompt, Context);
    }
}