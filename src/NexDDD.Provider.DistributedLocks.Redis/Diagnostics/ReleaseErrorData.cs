namespace NexDDD.Provider.DistributedLocks.Redis;

/// <summary>
/// 承载ReleaseErrorData 诊断事件的数据快照，供 DiagnosticListener 发布并由可观测性组件消费。
/// </summary>
public record ReleaseErrorData(Guid Id, string MethodName, Exception Exception);