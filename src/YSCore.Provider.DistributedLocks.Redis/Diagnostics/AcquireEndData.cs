namespace YSCore.Provider.DistributedLocks.Redis;

public record AcquireEndData(Guid Id, string Key, string MethodName);