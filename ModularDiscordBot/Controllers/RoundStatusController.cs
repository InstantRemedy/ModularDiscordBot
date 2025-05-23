using System.Globalization;
using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ModularDiscordBot.Configuration.Configurations;
using ModularDiscordBot.Constants;
using ModularDiscordBot.Plugins;
using ModularDiscordBot.Structures;

namespace ModularDiscordBot.Controllers;

public sealed class RoundStatusController : ControllerBase
{
    private readonly ILogger<RoundStatusController> _logger;
    private readonly RoundStatusConfiguration _configuration;
    private readonly ByondTopic _byondTopic = new();
    
    private ITextChannel? _channel;
    private IUserMessage? _embedMessage;
    private IUserMessage? _newRoundMessage;
    private GameState _lastGameState = GameState.Unknown;
    private ByondStatus? _lastStatus;
    private bool _isInitialized;
    private int _failedAttempts;
    
    public RoundStatusController(
        DiscordSocketClient client,
        ILogger<RoundStatusController> logger,
        RoundStatusConfiguration configuration) :
        base(client)
    {
        _logger = logger;
        _configuration = configuration;
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

        _lastStatus = ByoundStatusHelper.FromQuery(data);
        
        var embed = MakeEmbed(_lastStatus);
        
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
    
    private Embed MakeEmbed(ByondStatus status)
    {
        return status.GameState switch
        {
            GameState.Startup => MakeStartupEmbed(status).Build(),
            GameState.Lobby1 => MakeLobbyEmbed(status).Build(),
            GameState.Lobby2 => MakeLobbyEmbed(status).Build(),
            GameState.InGame => MakeInGameEmbed(status).Build(),
            GameState.EndGame => MakeEndGameEmbed(status).Build(),
            _ => throw new ArgumentException("Unknown game state")
        };
    }
    
    private EmbedBuilder MakeStartupEmbed(ByondStatus status)
    {
        return new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithTitle("Запуск сервера")
            .AddField("Id раунда", $"{status.RoundId}")
            .WithFooter(footer =>
            {
                footer.Text = "Сплетено пауком";
                footer.IconUrl = IconUrls.FooterIcon;
            })
            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
    }
    
    private EmbedBuilder MakeLobbyEmbed(ByondStatus status)
    {
        return new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("Лобби")
            .AddField("Id раунда", $"{status.RoundId}")
            .AddField("Количество игроков", $"{status.Players}")
            .WithFooter(footer =>
            {
                footer.Text = "Сплетено пауком";
                footer.IconUrl = IconUrls.FooterIcon;
            })
            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
    }
    
    private EmbedBuilder MakeInGameEmbed(ByondStatus status)
    {
        return new EmbedBuilder()
            .WithColor(Color.Green)
            .WithTitle("Раунд")
            .AddField("Id раунда", $"{status.RoundId}")
            .AddField("Количество игроков", $"{status.Players} игрок(ов)")
            .AddField("Время раунда", status.RoundDuration.ToString(@"hh\:mm"))
            .WithFooter(footer =>
            {
                footer.Text = "Сплетено пауком";
                footer.IconUrl = IconUrls.FooterIcon;
            })
            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
    }
    
    private EmbedBuilder MakeEndGameEmbed(ByondStatus status)
    {
        return new EmbedBuilder()
            .WithColor(Color.DarkMagenta)
            .WithTitle("Окончание раунда")
            .AddField("Раунд завершён", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            .AddField("Id раунда", $"{status.RoundId}")
            .WithFooter(footer =>
            {
                footer.Text = "Сплетено пауком";
                footer.IconUrl = IconUrls.FooterIcon;
            })
            .WithThumbnailUrl(IconUrls.ThumbnailIcon);
    }

    public override Task OnInitialized()
    {
        var channel = Client.GetChannel(_configuration.ChannelId);
        
        if(channel is not ITextChannel textChannel)
        {
            throw new ArgumentException("Channel is not a text channel");
        }
        
        _channel = textChannel;
        return Task.CompletedTask;
    }

    public override async Task OnShutdown()
    {
        if (!IsInitialized)
        {
            return;
        }
        
        if (_embedMessage is not null && _lastStatus is not null)
        {
            EmbedBuilder? embedBuilder = null;
            
            switch (_lastGameState)
            {
                case GameState.Startup:
                {
                    embedBuilder = MakeStartupEmbed(_lastStatus);
                    embedBuilder.WithColor(Color.DarkBlue);
                    embedBuilder.WithTitle("Бот отключён. Последний статус: Запуск сервера");
                }break;
                case GameState.Lobby1:
                case GameState.Lobby2:
                {
                    embedBuilder = MakeLobbyEmbed(_lastStatus);
                    embedBuilder.WithColor(Color.DarkBlue);
                    embedBuilder.WithTitle("Бот отключён. Последний статус: Лобби");
                }break;
                case GameState.InGame:
                {
                    embedBuilder = MakeInGameEmbed(_lastStatus);
                    embedBuilder.WithColor(Color.DarkBlue);
                    embedBuilder.WithTitle("Бот отключён. Последний статус: Раунд");
                }break;
            }
            
            if (embedBuilder is not null && _embedMessage is not null)
            {
                await _embedMessage.ModifyAsync(properties =>
                {
                    properties.Embed = embedBuilder.Build();
                });
            }
        }
    }
}