using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularDiscordBot.Controllers;

namespace ModularDiscordBot.Ioc;

public static class ServiceCollectionControllersExtensions
{
    public static IServiceCollection AddControllers(this IServiceCollection services)
    {
        var controllers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsSubclassOf(typeof(ControllerBase)) && !x.IsAbstract)
            .ToList();

        foreach (var controller in controllers)
        {
            services.AddSingleton(typeof(ControllerBase), provider => 
                provider.GetRequiredService(controller));
            services.AddSingleton(controller);
        }

        return services;
    }
}