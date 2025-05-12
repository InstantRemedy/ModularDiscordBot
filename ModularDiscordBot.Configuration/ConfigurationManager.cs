using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModularDiscordBot.Configuration.Attributes;

namespace ModularDiscordBot.Configuration;

public static class ConfigurationManager
{
    public static void Save<T>(T obj) 
    {
        var configAttribute = typeof(T).GetCustomAttribute<ConfigurationAttribute>();
        if (configAttribute == null)
            throw new InvalidOperationException("Type must be marked with ConfigurationAttribute.");

        string configName = configAttribute.Name;

        string jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never});
        File.WriteAllText($"config/{configName}.json", jsonString);
    }

    public static object? Load(Type type)
    {
        var configAttribute = type.GetCustomAttribute<ConfigurationAttribute>();
        if (configAttribute == null)
            throw new InvalidOperationException("Type must be marked with ConfigurationAttribute.");

        string configName = configAttribute.Name;

        if (!File.Exists($"config/{configName}.json"))
        {
            return Activator.CreateInstance(type)!;
        }

        string jsonString = File.ReadAllText($"config/{configName}.json");

        return JsonSerializer.Deserialize(jsonString, type)!;
    }
    
    public static T? Load<T>()
    {
        return (T?)Load(typeof(T));
    }
}