using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等过期记录清理任务。
/// </summary>
internal sealed class IdempotencyCleanupHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<IdempotencyOptions> options,
    ILogger<IdempotencyCleanupHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanupInterval = options.Value.CleanupInterval;
        if (cleanupInterval <= TimeSpan.Zero)
        {
            return;
        }

        using var timer = new PeriodicTimer(cleanupInterval);
        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
                var deleted = await store.DeleteExpiredAsync(options.Value.CleanupBatchSize, stoppingToken);
                if (deleted > 0)
                {
                    logger.LogInformation("幂等记录清理完成，本次清理 {Count} 条。", deleted);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "幂等记录清理失败。");
            }
        }
    }
}
