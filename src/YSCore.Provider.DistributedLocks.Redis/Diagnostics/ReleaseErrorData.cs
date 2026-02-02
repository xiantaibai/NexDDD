namespace YSCore.Provider.DistributedLocks.Redis;

public record ReleaseErrorData(Guid Id, string MethodName, Exception Exception);