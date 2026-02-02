namespace YSCore.Provider.DistributedLocks.Redis;

public record AcquireErrorData(Guid Id, string Key, string MethodName, Exception Exception);