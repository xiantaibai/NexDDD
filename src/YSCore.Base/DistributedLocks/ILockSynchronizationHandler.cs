namespace YSCore.Base.DistributedLocks;

public interface ILockSynchronizationHandler : IDisposable, IAsyncDisposable
{
    CancellationToken HandleLostToken { get; }
}