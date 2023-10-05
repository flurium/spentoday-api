using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Lib;

/// <summary>
/// Background queue implemented with channels.
/// Do tasks in background.
/// Can be used accross different projects.
/// </summary>
public class BackgroundQueue
{
    private readonly Channel<Func<IServiceProvider, Task>> queue;

    public BackgroundQueue()
    {
        var options = new UnboundedChannelOptions
        {
            // Because service is singleton.
            SingleReader = true
        };
        queue = Channel.CreateUnbounded<Func<IServiceProvider, Task>>(options);
    }

    public async Task Enqueue(Func<IServiceProvider, Task> task)
    {
        await queue.Writer.WriteAsync(task);
    }

    public class HostedService : BackgroundService
    {
        private readonly ILogger<HostedService> logger;
        private readonly BackgroundQueue queue;
        private readonly IServiceProvider serviceProvider;

        public HostedService(
            ILogger<HostedService> logger,
            BackgroundQueue queue,
            IServiceProvider serviceProvider
        )
        {
            this.logger = logger;
            this.queue = queue;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background queue Hostend service is started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await queue.queue.Reader.ReadAsync(stoppingToken);
                try
                {
                    await job(serviceProvider);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Exception appeared during execution of Background queue job: {nameof(job)}.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background queue Hosted Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}