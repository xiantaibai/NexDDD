using System.Diagnostics;

namespace YSCore.Provider.DistributedLocks.Redis;

public static class YSCoreRedisDiagnosticListenerNames
{
    public static readonly DiagnosticListener DiagnosticListener =
        new(DiagnosticListenerName);

    public const string DiagnosticListenerName = "YSCore.RedisLockDiagnosticListener";

    public const string AcquireBegin = "YSCore.RedisLock.AcquireBegin";
    public const string AcquireEnd = "YSCore.RedisLock.AcquireEnd";
    public const string AcquireError = "YSCore.RedisLock.AcquireErrorData";
    public const string ReleaseBegin = "YSCore.RedisLock.ReleaseBegin";
    public const string ReleaseEnd = "YSCore.RedisLock.ReleaseEnd";
    public const string ReleaseError = "YSCore.RedisLock.ReleaseError";
}