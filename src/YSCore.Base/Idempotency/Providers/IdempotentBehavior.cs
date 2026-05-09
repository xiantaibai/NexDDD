using MediatR;
using Microsoft.Extensions.Options;
using YSCore.Base.Events;

namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等处理行为。
/// </summary>
/// <typeparam name="TRequest">请求类型。</typeparam>
/// <typeparam name="TResponse">返回类型。</typeparam>
public sealed class IdempotentBehavior<TRequest, TResponse>(
    IEnumerable<IIdempotentCommand<TRequest>> idempotentCommands,
    IIdempotencyStore idempotencyStore,
    IOptions<IdempotencyOptions> options)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var count = idempotentCommands.Count();
        if (count == 0)
        {
            return await next();
        }

        if (count > 1)
        {
            throw new InvalidOperationException("同一个命令只允许注册一个 IIdempotentCommand<TRequest>。");
        }

        var idempotentCommand = idempotentCommands.First();
        var settings = await idempotentCommand.GetIdempotencyAsync(request, cancellationToken);
        var record = IdempotencyKeyHelper.CreateRecord(settings, options);
        var beginResult = await idempotencyStore.TryBeginAsync(record, cancellationToken);
        if (beginResult.IsCompleted)
        {
            throw new IdempotencyException("请勿重复提交相同请求", record.TypeCode, record.KeyHash);
        }

        if (beginResult.IsProcessing)
        {
            throw new IdempotencyException("请求正在处理中", record.TypeCode, record.KeyHash);
        }

        try
        {
            var response = await next();

            await idempotencyStore.MarkCompletedAsync(record.TypeCode, record.KeyHash, beginResult.RequestId, cancellationToken);

            return response;
        }
        catch
        {
            // 业务失败
            // 删除幂等锁
            // 允许重新提交
            try
            {
                await idempotencyStore.RemoveAsync(
                    record.TypeCode,
                    record.KeyHash,
                    beginResult.RequestId,
                    cancellationToken);
            }
            catch
            {
                // 不覆盖业务异常
            }

            throw;
        }
    }
}
