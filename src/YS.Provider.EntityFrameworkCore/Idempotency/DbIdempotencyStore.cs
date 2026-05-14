using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yitter.IdGenerator;
using YSCore.Base.Idempotency;

namespace YSCore.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 基于数据库的幂等存储。
/// 该存储依赖当前请求中的 DbContext，并建议放在事务行为内部执行。
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public sealed class DbIdempotencyStore<TDbContext> : IIdempotencyStore
    where TDbContext : DbContext
{
    /// <summary>
    /// 当前事务 DbContext
    /// </summary>
    private readonly TDbContext _dbContext;

    /// <summary>
    /// 独立事务
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory;

    public DbIdempotencyStore(
        IServiceScopeFactory scopeFactory, TDbContext dbContext)
    {
        _scopeFactory = scopeFactory;
        _dbContext = dbContext;
    }

    public async Task<IdempotencyBeginResult> TryBeginAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TDbContext>();

        var utcTime = DateTime.UtcNow;

        var entity = IdempotencyRecordEntity.Create(record);
        string requestId = entity.RequestId;

        await dbContext.Set<IdempotencyRecordEntity>().AddAsync(entity, cancellationToken);
        try
        {
            var rows = dbContext.SaveChanges();
            return IdempotencyBeginResult
                .SuccessResult(requestId);
        }
        catch (DbUpdateException ex)
        {
            // 非唯一键异常
            if (!IsUniqueConstraintException(ex))
            {
                throw;
            }
            // 清理跟踪
            dbContext.Entry(entity).State = EntityState.Detached;
        }

        var current =
          await dbContext.Set<IdempotencyRecordEntity>()
              .AsNoTracking()
              .FirstAsync(
                  x => x.TypeCode == record.TypeCode
                    && x.KeyHash == record.KeyHash,
                  cancellationToken);

        if (current.Status == IdempotencyStatus.Completed)
        {
            return IdempotencyBeginResult
                .CompletedResult();
        }

        // Processing 未超时

        if (current.ExpireTime > utcTime)
        {
            return IdempotencyBeginResult
                .ProcessingResult();
        }

        // 超时恢复抢占
        var affectedRows =
            await dbContext.Set<IdempotencyRecordEntity>()
                .Where(x =>
                    x.TypeCode == record.TypeCode
                    && x.KeyHash == record.KeyHash
                    && x.Status == IdempotencyStatus.Processing
                    && x.ExpireTime <= utcTime)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(
                            x => x.RequestId,
                            requestId)
                        .SetProperty(
                    x => x.ExpireTime,
                            entity.ExpireTime)
                        .SetProperty(
                            x => x.LastModifyTime,
                            utcTime),
                    cancellationToken);

        if (affectedRows > 0)
        {
            return IdempotencyBeginResult
                .SuccessResult(requestId);
        }

        return IdempotencyBeginResult
            .ProcessingResult();
    }

    public async Task MarkCompletedAsync(string typeCode, string keyHash, string requestId, CancellationToken cancellationToken = default)
    {
        //using var scope = _scopeFactory.CreateScope();

        //var dbContext =
        //    scope.ServiceProvider
        //        .GetRequiredService<TDbContext>();

        await _dbContext.Set<IdempotencyRecordEntity>()
           .Where(x =>
               x.TypeCode == typeCode
               && x.KeyHash == keyHash
               && x.RequestId == requestId)
           .ExecuteUpdateAsync(
               s => s
                   .SetProperty(
                       x => x.Status,IdempotencyStatus.Completed)
                   .SetProperty(
                       x => x.LastModifyTime,
                       DateTime.UtcNow),
               cancellationToken);
    }

    public async Task RemoveAsync(string typeCode, string keyHash, string requestId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TDbContext>();

        await dbContext.Set<IdempotencyRecordEntity>()
            .Where(x => x.TypeCode == typeCode && x.KeyHash == keyHash && x.RequestId == requestId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var size = batchSize <= 0 ? 500 : batchSize;

        using var scope = _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TDbContext>();

        return await dbContext.Set<IdempotencyRecordEntity>()
            .Where(x => x.ExpireTime <= now && x.Status == IdempotencyStatus.Processing)
            .OrderBy(x => x.ExpireTime)
            .Take(size)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> DeleteCompletedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var size = batchSize <= 0 ? 500 : batchSize;

        using var scope = _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TDbContext>();

        return await dbContext.Set<IdempotencyRecordEntity>()
            .Where(x => x.ExpireTime <= now && x.Status == IdempotencyStatus.Completed)
            .OrderBy(x => x.ExpireTime)
            .Take(size)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// 判断是否重复
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    private static bool IsUniqueConstraintException(
       DbUpdateException ex)
    {
        var message = ex.InnerException?.Message
                      ?? ex.Message;

        message = message.ToLowerInvariant();

        return message.Contains("duplicate")
               || message.Contains("unique")
               || message.Contains("constraint");
    }
}
