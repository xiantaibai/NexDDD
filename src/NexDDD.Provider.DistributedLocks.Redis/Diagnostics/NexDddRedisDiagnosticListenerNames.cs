using System.Diagnostics;

namespace NexDDD.Provider.DistributedLocks.Redis;

/// <summary>
/// 负责分布式锁模块中的 NexDddRedisDiagnosticListenerNames 能力，并限定其与上层调用者和基础设施组件的协作边界。
/// </summary>
public static class NexDddRedisDiagnosticListenerNames
{
    public static readonly DiagnosticListener DiagnosticListener =
        new(DiagnosticListenerName);

    public const string DiagnosticListenerName = "NexDDD.RedisLockDiagnosticListener";

    public const string AcquireBegin = "NexDDD.RedisLock.AcquireBegin";
    public const string AcquireEnd = "NexDDD.RedisLock.AcquireEnd";
    public const string AcquireError = "NexDDD.RedisLock.AcquireErrorData";
    public const string ReleaseBegin = "NexDDD.RedisLock.ReleaseBegin";
    public const string ReleaseEnd = "NexDDD.RedisLock.ReleaseEnd";
    public const string ReleaseError = "NexDDD.RedisLock.ReleaseError";
}