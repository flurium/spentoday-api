using System.Collections.Concurrent;

namespace Backend.Services;

/// <summary>
/// Conatins functions to run in background.
/// Allow to add new function to run.
/// </summary>
public class BackgroundQueue
{
    private readonly BlockingCollection<Func<IServiceProvider, Task>> queue = new();
    private readonly ILogger<BackgroundQueue> logger;

    public BackgroundQueue(ILogger<BackgroundQueue> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Add function to background queue.
    /// To add synchronous functions just return Task.CompletedTask at the end.
    /// </summary>
    /// <param name="task">
    /// Function that accept IServiceProvider (to get services if need) and return nothing.
    /// </param>
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

    /// <summary>
    /// Run functions from BackgrondQueue in background.
    /// </summary>
    public class Runner : IHostedService
    {
        private readonly ILogger<Runner> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly BackgroundQueue backgroundQueue;

        private Task? backgroundTask = null;
        private CancellationTokenSource? cancellationTokenSource = null;

        public Runner(ILogger<Runner> logger, IServiceProvider serviceProvider, BackgroundQueue backgroundQueue)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.backgroundQueue = backgroundQueue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Background is started.");

            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            backgroundTask = Task.Run(() => Run(cancellationTokenSource.Token), cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Background is stopping.");

            if (cancellationTokenSource != null && backgroundTask != null)
            {
                cancellationTokenSource.Cancel();
                await backgroundTask;
            }

            logger.LogInformation("Background has stopped.");
        }

        private async Task Run(CancellationToken stoppingToken)
        {
            foreach (var task in backgroundQueue.queue.GetConsumingEnumerable(stoppingToken))
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
        }
    }
}