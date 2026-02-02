namespace YSCore.Provider.DistributedLocks.Redis;

public record ReleaseEndData(Guid Id, string MethodName);