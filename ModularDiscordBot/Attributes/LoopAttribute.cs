namespace ModularDiscordBot.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class LoopAttribute : Attribute
{
    public TimeSpan Interval { get; }

    public LoopAttribute(int milliseconds)
    {
        Interval = TimeSpan.FromMilliseconds(milliseconds);
    }

    public LoopAttribute(int hours, int minutes, int seconds)
    {
        Interval = new TimeSpan(hours, minutes, seconds);
    }
}