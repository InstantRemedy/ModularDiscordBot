using System.Reflection;
using ModularDiscordBot.Configuration.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModularDiscordBot.Configuration;

public abstract class Configuration
{
    public Configuration()
    {
        LoadConfiguration();
    }
    
    public string Name
    {
        get
        {
            var attribute = GetType().GetCustomAttribute<ConfigurationAttribute>();
            return attribute?.Name ?? GetType().Name;
        }
    }

    public string ConfigPath 
        => Path.Combine(Environment.CurrentDirectory, "config", $"{Name}.json");
    
    public void SaveConfiguration()
    {
        var properties = GetType().GetProperties();
        var json = new JObject();
        
        foreach (var property in properties)
        {
            var propertyAttribute = property.GetCustomAttribute<PropertyConfiguration>();
            
            if(propertyAttribute == null)
            {
                continue;
            }
            
            var propertyName = propertyAttribute.Name;
            var propertyValue = property.GetValue(this);
            
            json[propertyName] = JToken.FromObject(propertyValue!);
        }
        
        File.WriteAllText(ConfigPath, json.ToString(Formatting.Indented));        
    }
    
    public async Task SaveConfigurationAsync()
    {
        var properties = GetType().GetProperties();
        var json = new JObject();
        
        foreach (var property in properties)
        {
            var propertyAttribute = property.GetCustomAttribute<PropertyConfiguration>();
            
            if(propertyAttribute == null)
            {
                continue;
            }
            
            var propertyName = propertyAttribute.Name;
            var propertyValue = property.GetValue(this);
            
            json[propertyName] = JToken.FromObject(propertyValue!);
        }
        
        await File.WriteAllTextAsync(ConfigPath, json.ToString(Formatting.Indented));        
    }
    
    public void LoadConfiguration()
    {
        if (!File.Exists(ConfigPath))
        {
            SaveConfiguration();
            return;
        }
        
        var json = File.ReadAllText(ConfigPath);
        var properties = GetType().GetProperties();
        var jObject = JObject.Parse(json);
        
        foreach (var property in properties)
        {
            var propertyAttribute = property.GetCustomAttribute<PropertyConfiguration>();
            
            if(propertyAttribute == null)
            {
                continue;
            }
            
            var propertyName = propertyAttribute.Name;
            var propertyValue = jObject[propertyName];
            
            if (propertyValue == null)
            {
                continue;
            }
            
            property.SetValue(this, propertyValue.ToObject(property.PropertyType));
        }
    }
    
    public async Task LoadConfigurationAsync()
    {
        if (!File.Exists(ConfigPath))
        {
            await SaveConfigurationAsync();
            return;
        }
        
        var json = await File.ReadAllTextAsync(ConfigPath);
        var properties = GetType().GetProperties();
        var jObject = JObject.Parse(json);
        
        foreach (var property in properties)
        {
            var propertyAttribute = property.GetCustomAttribute<PropertyConfiguration>();
            var propertyName = propertyAttribute?.Name ?? property.Name;
            var propertyValue = jObject[propertyName];
            
            if (propertyValue == null)
            {
                continue;
            }
            
            property.SetValue(this, propertyValue.ToObject(property.PropertyType));
        }
    }
}