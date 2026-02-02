namespace YSCore.Provider.DistributedLocks.Redis;

public record ReleaseBeginData(Guid Id, string MethodName);