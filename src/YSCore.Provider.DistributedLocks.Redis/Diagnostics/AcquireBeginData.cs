namespace YSCore.Provider.DistributedLocks.Redis;

public record AcquireBeginData(Guid Id, string Key, string MethodName);