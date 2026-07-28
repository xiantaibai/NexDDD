using Medallion.Threading;
using NexDDD.Core.DistributedLocks;

namespace NexDDD.Provider.DistributedLocks.Redis;

/// <summary>
/// 负责分布式锁流程的处理与编排，协调上游请求和下游服务完成当前用例。
/// </summary>
internal sealed class RedisLockSynchronizationHandler : ILockSynchronizationHandler
{
    private readonly IDistributedSynchronizationHandle _handle;

    public RedisLockSynchronizationHandler(IDistributedSynchronizationHandle handle)
    {
        _handle = handle;
    }

    public CancellationToken HandleLostToken => _handle.HandleLostToken;

    public void Dispose()
    {
        Guid id = Guid.NewGuid();
        try
        {
            WriteReleaseBeginData(new ReleaseBeginData(id, "Dispose"));
            _handle.Dispose();
            WriteReleaseEndData(new ReleaseEndData(id, $"Dispose"));
        }
        catch (Exception ex)
        {
            WriteReleaseErrorData(new ReleaseErrorData(id, "Dispose", ex));
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Guid id = Guid.NewGuid();
        try
        {
            WriteReleaseBeginData(new ReleaseBeginData(id, "DisposeAsync"));
            await _handle.DisposeAsync();
            WriteReleaseEndData(new ReleaseEndData(id, $"Dispose"));
        }
        catch (Exception ex)
        {
            WriteReleaseErrorData(new ReleaseErrorData(id, "DisposeAsync", ex));
            throw;
        }
    }


    void WriteReleaseBeginData(ReleaseBeginData data)
    {
        if (NexDddRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NexDddRedisDiagnosticListenerNames
                .ReleaseBegin))
        {
            NexDddRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NexDddRedisDiagnosticListenerNames.ReleaseBegin, data);
        }
    }

    void WriteReleaseEndData(ReleaseEndData data)
    {
        if (NexDddRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NexDddRedisDiagnosticListenerNames
                .ReleaseEnd))
        {
            NexDddRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NexDddRedisDiagnosticListenerNames.ReleaseEnd, data);
        }
    }

    void WriteReleaseErrorData(ReleaseErrorData data)
    {
        if (NexDddRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NexDddRedisDiagnosticListenerNames
                .ReleaseError))
        {
            NexDddRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NexDddRedisDiagnosticListenerNames.ReleaseError, data);
        }
    }
}