using System.Diagnostics;

namespace YSCore.Observability.Abstractions;

/// <summary>
/// 表示一个可观测的业务操作，负责记录链路属性以及成功或失败结果。
/// </summary>
public interface IYsObservabilityOperation : IDisposable
{
    Activity? Activity { get; }

    IYsObservabilityOperation SetTag(string name, object? value);

    void SetSuccess();

    void SetError(Exception exception);
}
