using NexDDD.Core.Events;

namespace NexDDD.Core.Idempotency;

/// <summary>
/// 命令幂等配置提供器。
/// </summary>
/// <typeparam name="TCommand">命令类型。</typeparam>
public interface IIdempotentCommand<in TCommand> where TCommand : IBaseCommand
{
    /// <summary>
    /// 获取幂等配置。
    /// </summary>
    /// <param name="command">当前命令。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns></returns>
    Task<IdempotencySettings> GetIdempotencyAsync(TCommand command, CancellationToken cancellationToken = default);
}
