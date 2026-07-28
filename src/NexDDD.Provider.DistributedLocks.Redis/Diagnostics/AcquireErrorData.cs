namespace NexDDD.Provider.DistributedLocks.Redis;

/// <summary>
/// 承载AcquireErrorData 诊断事件的数据快照，供 DiagnosticListener 发布并由可观测性组件消费。
/// </summary>
public record AcquireErrorData(Guid Id, string Key, string MethodName, Exception Exception);