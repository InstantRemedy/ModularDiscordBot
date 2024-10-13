namespace ModularDiscordBot.Configuration.Attributes;

public sealed class PropertyConfiguration : Attribute
{
    public string Name { get; }
    
    public PropertyConfiguration(string name)
    {
        Name = name;
    }
}