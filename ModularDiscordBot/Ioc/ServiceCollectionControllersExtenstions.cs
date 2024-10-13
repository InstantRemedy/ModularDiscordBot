using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularDiscordBot.Interfaces;

namespace ModularDiscordBot.Ioc;

public static class ServiceCollectionControllersExtenstions
{
    public static IServiceCollection AddControllers(this IServiceCollection services)
    {
        var controllers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IBotController)) && !x.IsInterface && !x.IsAbstract)
            .ToList();

        foreach (var controller in controllers)
        {
            services.AddSingleton(controller);
        }

        return services;
    }
}