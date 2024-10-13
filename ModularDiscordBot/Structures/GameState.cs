namespace ModularDiscordBot.Structures;

public enum GameState
{
    Unknown = -1337,
    Startup = 0,
    Lobby1 = 1,
    Lobby2 = 2,
    InGame = 3,
    EndGame = 4
}

public static class GameStateExtensions
{
    public static string ToFriendlyString(this GameState state)
    {
        return state switch
        {
            GameState.Unknown => "Unknown",
            GameState.Startup => "Startup",
            GameState.Lobby1 => "Lobby",
            GameState.Lobby2 => "Lobby",
            GameState.InGame => "InGame",
            GameState.EndGame => "EndGame",
            _ => "Unknown"
        };
    }
}