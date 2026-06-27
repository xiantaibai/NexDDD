using System.Diagnostics;

namespace YSCore.Observability.Abstractions;

/// <summary>
/// 定义创建业务操作链路的统一入口，并负责关联标准操作指标。
/// </summary>
public interface IYsObservability
{
    IYsObservabilityOperation StartOperation(
        string operationName,
        ActivityKind kind = ActivityKind.Internal,
        IEnumerable<KeyValuePair<string, object?>>? tags = null);
}
