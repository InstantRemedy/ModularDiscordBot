using System.Globalization;
using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Constants;
using ModularDiscordBot.Interfaces;
using ModularDiscordBot.Plugins;
using ModularDiscordBot.Structures;

namespace ModularDiscordBot.Controllers;

public sealed class RoundStatusController : IBotController
{
    private readonly ILogger<RoundStatusController> _logger;
    private readonly RoundStatusConfiguration _configuration;
    private readonly ByondTopic _byondTopic = new();
    
    private ITextChannel? _channel;
    private IUserMessage? _embedMessage;
    private IUserMessage? _newRoundMessage;
    private GameState _lastGameState = GameState.Unknown;
    private bool _isInitialized;
    private int _failedAttempts;

    public RoundStatusController(
        DiscordSocketClient client,
        ILogger<RoundStatusController> logger,
        RoundStatusConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        var channel = client.GetChannel(_configuration.ChannelId);
        
        if(channel is not ITextChannel textChannel)
        {
            throw new ArgumentException("Channel is not a text channel");
        }
        
        _channel = textChannel;
    }

    public async Task CheckStatusAsync()
    {
        if (_channel is null)
        {
            _logger.LogError("Channel is not initialized");
            return;
        }

        var data = await TryQueryStatusAsync();
        if(data is null)
        {
            return;
        }
        
        if(!data.TryGetValue("round_duration" , out var roundDurationToken) ||
           !data.TryGetValue("gamestate", out var gameStateToken))
        {
            _logger.LogError("Missing required fields in response");
            return;
        }

        var currentTime = int.Parse(roundDurationToken);
        var gameState = (GameState)Enum.Parse(typeof(GameState), gameStateToken, true);
        var embed = MakeEmbed(data, currentTime, gameState);
        
        LogState(currentTime, gameState);
        await CreateOrModifyMessageAsync(embed, gameState);
    }
    
    private async Task<Dictionary<string, string>?> TryQueryStatusAsync()
    {
        _logger.LogInformation($"Querying round status: {_configuration.Host}:{_configuration.Port}");
        
        try
        {
            var data = await _byondTopic.QueryStatus(_configuration.Host, _configuration.Port);
            if(data is null)
            {
                throw new Exception("Failed to query round status");
            }
            
            _failedAttempts = 0;
            return data;
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to query round status: {e.Message}");
            await HandleQueryException();
            return null;
        }
    }
    
    private void LogState(int currentTime, GameState gameState)
    {
        if (gameState == GameState.InGame)
        {
            _logger.LogInformation(
                new StringBuilder()
                    .Append($"Game state: {gameState.ToFriendlyString()}. ")
                    .Append($"Time: {TimeSpan.FromSeconds(currentTime).ToString(@"hh\:mm")}")
                    .ToString());
        }
        else
        {
            _logger.LogInformation($"Game state: {gameState.ToFriendlyString()}");
        }
    }

    private async Task HandleQueryException()
    {
        _failedAttempts++;

        if (_failedAttempts >= 5)
        {
            _isInitialized = false;
            _failedAttempts = 0;
                
            if(_lastGameState != GameState.EndGame && 
               _lastGameState != GameState.Unknown)
            {
                _lastGameState = GameState.Unknown;

                if (_embedMessage is not null)
                {
                    await _embedMessage.DeleteAsync();
                    _embedMessage = null;
                }

                if (_newRoundMessage is not null)
                {
                    await _newRoundMessage.DeleteAsync();
                    _newRoundMessage = null;
                }
            }
        }
    }
    
    private async Task CreateOrModifyMessageAsync(Embed embed, GameState gameState)
    {
        if (!_isInitialized ||
            (gameState == GameState.Startup &&
             gameState != _lastGameState &&
             _lastGameState == GameState.EndGame))
        {
            _embedMessage = await _channel!.SendMessageAsync(embed: embed);
            _newRoundMessage = await _channel.SendMessageAsync(
                "<@&1227295722123296799> Новый раунд```byond://rockhill-game.ru:51143```");
            _isInitialized = true;
        }
        else
        {
            await _embedMessage!.ModifyAsync(properties => properties.Embed = embed);
        }
        
        _lastGameState = gameState;
    }
    
    private Embed MakeEmbed(Dictionary<string, string> responseData, int currentTime, GameState gameState)
    {
        EmbedBuilder embedBuilder = new EmbedBuilder();
        switch (gameState)
        {
            case GameState.Startup:
                embedBuilder.WithColor(Color.Orange)
                            .WithTitle("Раунд")
                            .WithDescription("Запуск сервера")
                            .WithFooter(footer =>
                            {
                                footer.Text = "Сплетено пауком";
                                footer.IconUrl = IconUrls.FooterIcon;
                            })
                            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
                break;

            case GameState.Lobby1:
            case GameState.Lobby2:
                embedBuilder.WithColor(Color.Blue)
                            .WithTitle("Раунд")
                            .WithDescription("Лобби")
                            .WithFooter(footer =>
                            {
                                footer.Text = "Сплетено пауком";
                                footer.IconUrl = IconUrls.FooterIcon;
                            })
                            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
                break;

            case GameState.InGame:
                embedBuilder.WithColor(Color.Green)
                            .WithTitle("Раунд")
                            .WithDescription("Идёт раунд")
                            .AddField("Количество игроков", $"{responseData["players"]} игрок(ов)", true)
                            .AddField("Время раунда", TimeSpan.FromSeconds(currentTime).ToString(@"hh\:mm"), true)
                            .WithFooter(footer =>
                            {
                                footer.Text = "Сплетено пауком";
                                footer.IconUrl = IconUrls.FooterIcon;
                            })
                            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
                break;

            case GameState.EndGame:
                embedBuilder.WithColor(Color.DarkMagenta)
                            .WithTitle("Раунд")
                            .WithDescription("Окончание раунда")
                            .AddField("Раунд завершён", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), true)
                            .WithFooter(footer =>
                            {
                                footer.Text = "Сплетено пауком";
                                footer.IconUrl = IconUrls.FooterIcon;
                            })
                            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
                break;

            default:
                throw new ArgumentException("Unknown game state");
        }

        return embedBuilder.Build();
    }
}