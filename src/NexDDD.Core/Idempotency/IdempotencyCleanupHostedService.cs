using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NexDDD.Core.Idempotency;

/// <summary>
/// 幂等记录清理任务。
/// </summary>
internal sealed class IdempotencyCleanupHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<IdempotencyOptions> options,
    ILogger<IdempotencyCleanupHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var expiredCleanupTask = RunExpiredCleanupAsync(stoppingToken);
        var completedCleanupTask = RunCompletedCleanupAsync(stoppingToken);

        await Task.WhenAll(expiredCleanupTask, completedCleanupTask);
    }

    private Task RunExpiredCleanupAsync(CancellationToken stoppingToken)
    {
        return RunCleanupAsync(
            options.Value.CleanupInterval,
            "过期幂等记录清理完成，本次清理 {Count} 条。",
            "过期幂等记录清理失败。",
            (store, cancellationToken) => store.DeleteExpiredAsync(options.Value.CleanupBatchSize, cancellationToken),
            stoppingToken);
    }

    private Task RunCompletedCleanupAsync(CancellationToken stoppingToken)
    {
        return RunCleanupAsync(
            options.Value.CompletedCleanupInterval,
            "已完成幂等记录清理完成，本次清理 {Count} 条。",
            "已完成幂等记录清理失败。",
            (store, cancellationToken) => store.DeleteCompletedAsync(options.Value.CleanupBatchSize, cancellationToken),
            stoppingToken);
    }

    private async Task RunCleanupAsync(
        TimeSpan interval,
        string successMessage,
        string errorMessage,
        Func<IIdempotencyStore, CancellationToken, Task<int>> cleanupAction,
        CancellationToken stoppingToken)
    {
        if (interval <= TimeSpan.Zero)
        {
            return;
        }

        using var timer = new PeriodicTimer(interval);
        try
        {
            while (!stoppingToken.IsCancellationRequested
                   && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
                    var deleted = await cleanupAction(store, stoppingToken);
                    if (deleted > 0)
                    {
                        logger.LogInformation(successMessage, deleted);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, errorMessage);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
