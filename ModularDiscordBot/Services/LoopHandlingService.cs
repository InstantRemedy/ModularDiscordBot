using System.Reflection;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ModularDiscordBot.Attributes;
using ModularDiscordBot.Modules;

namespace ModularDiscordBot.Services;

public class LoopHandlingService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoopHandlingService> _logger;
    private readonly List<Task> _tasks = new List<Task>();

    public LoopHandlingService(
        DiscordSocketClient client,
        IServiceProvider serviceProvider,
        ILogger<LoopHandlingService> logger)
    {
        _client = client;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Ожидаем, пока клиент не будет готов
        await WaitForClientReadyAsync(stoppingToken);

        // Инициализируем циклы
        InitializeLoops(stoppingToken);

        // Ожидаем завершения всех задач или отмены токена
        await Task.WhenAny(_tasks.Append(Task.Delay(Timeout.Infinite, stoppingToken)));
    }

    private async Task WaitForClientReadyAsync(CancellationToken stoppingToken)
    {
        if (_client.CurrentUser != null)
            return;

        var tcs = new TaskCompletionSource<bool>();

        Task ClientReady()
        {
            tcs.SetResult(true);
            return Task.CompletedTask;
        }

        _client.Ready += ClientReady;

        try
        {
            await tcs.Task.WaitAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Ожидание готовности клиента было отменено.");
        }
        finally
        {
            _client.Ready -= ClientReady;
        }
    }

    private void InitializeLoops(CancellationToken stoppingToken)
    {
        // Получаем все типы, наследующиеся от LoopModule
        var loopModuleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(LoopModule)) && !t.IsAbstract);

        foreach (var moduleType in loopModuleTypes)
        {
            try
            {
                // Создаем экземпляр модуля через IServiceProvider
                var moduleInstance = ActivatorUtilities.CreateInstance(_serviceProvider, moduleType);

                // Получаем методы, помеченные LoopAttribute
                var methods = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(m => m.GetCustomAttribute<LoopAttribute>() != null);

                foreach (var method in methods)
                {
                    var loopAttr = method.GetCustomAttribute<LoopAttribute>();
                    StartLoop(method, moduleInstance, loopAttr!.Interval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при инициализации модуля {moduleType.Name}.");
            }
        }
    }

    private void StartLoop(MethodInfo method, object moduleInstance, TimeSpan interval, CancellationToken stoppingToken)
    {
        Task task = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = method.Invoke(moduleInstance, null);
                    if (result is Task taskResult)
                    {
                        await taskResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка в цикле {method.Name}.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Задача была отменена, выходим из цикла
                    break;
                }
            }
        }, stoppingToken);

        _tasks.Add(task);
    }
}
