using Medallion.Threading;
using YSCore.Base.DistributedLocks;

namespace YSCore.Provider.DistributedLocks.Redis;

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
        if (YSCoreRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(YSCoreRedisDiagnosticListenerNames
                .ReleaseBegin))
        {
            YSCoreRedisDiagnosticListenerNames.DiagnosticListener.Write(
                YSCoreRedisDiagnosticListenerNames.ReleaseBegin, data);
        }
    }

    void WriteReleaseEndData(ReleaseEndData data)
    {
        if (YSCoreRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(YSCoreRedisDiagnosticListenerNames
                .ReleaseEnd))
        {
            YSCoreRedisDiagnosticListenerNames.DiagnosticListener.Write(
                YSCoreRedisDiagnosticListenerNames.ReleaseEnd, data);
        }
    }

    void WriteReleaseErrorData(ReleaseErrorData data)
    {
        if (YSCoreRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(YSCoreRedisDiagnosticListenerNames
                .ReleaseError))
        {
            YSCoreRedisDiagnosticListenerNames.DiagnosticListener.Write(
                YSCoreRedisDiagnosticListenerNames.ReleaseError, data);
        }
    }
}