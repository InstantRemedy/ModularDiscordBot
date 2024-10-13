namespace ModularDiscordBot.Structures;

public enum BotStatus
{
    Off,
    Ready
}

public static class BotStatusHelper
{
    public static string ToStatusString(this BotStatus status)
    {
        return status switch
        {
            BotStatus.Off => "Дремлет",
            BotStatus.Ready => "Плетёт интриги",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}