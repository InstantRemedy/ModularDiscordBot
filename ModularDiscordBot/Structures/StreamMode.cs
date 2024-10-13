namespace ModularDiscordBot.Structures;

public enum StreamMode
{
    Stream,
    NoStream
}

public static class StreamModeExtensions
{
    public static string ToStreamString(this StreamMode mode)
    {
        return mode switch
        {
            StreamMode.Stream => "stream",
            StreamMode.NoStream => "no_stream",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}

public static class StreamModeHelper
{
    public static StreamMode FromString(string mode)
    {
        return mode switch
        {
            "stream" => StreamMode.Stream,
            "no_stream" => StreamMode.NoStream,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}