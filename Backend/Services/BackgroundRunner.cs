using System.Collections.Concurrent;

namespace Backend.Services;

/// <summary>
/// Run functions in background.
/// </summary>
public class BackgroundRunner : BackgroundService
{
    private readonly BlockingCollection<Func<IServiceProvider, Task>> queue = new();
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;

    public BackgroundRunner(ILogger logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Background is started.");

        foreach (var task in queue.GetConsumingEnumerable(stoppingToken))
        {
            try
            {
                await task(serviceProvider);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception appeared during execution of Background task.");
            }
        }

        logger.LogInformation("Background is shutting down.");
    }

    public void Enqueue(Func<IServiceProvider, Task> task)
    {
        try
        {
            queue.Add(task);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Can't add task to background queue.");
        }
    }
}