namespace ModularDiscordBot.Configuration.Attributes;

public sealed class ConfigurationAttribute : Attribute
{
    public string Name { get; }
    
    public ConfigurationAttribute(string name)
    {
        Name = name;
    }
}