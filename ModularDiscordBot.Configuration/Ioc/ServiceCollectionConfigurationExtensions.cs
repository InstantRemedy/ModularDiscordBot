using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace ModularDiscordBot.Configuration.Ioc;

public static class ServiceCollectionConfigurationExtensions
{
    public static IServiceCollection AddConfigurations(this IServiceCollection services)
    {
        var configurationType = typeof(Configuration);
        var configurations = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => configurationType.IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var config in configurations)
        {
            services.AddSingleton(config);
        }

        return services;
    }
}