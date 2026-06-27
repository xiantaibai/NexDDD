using Microsoft.Extensions.Logging;

namespace YSCore.Observability.Extensions;

/// <summary>
/// 为标准 ILogger 提供符合 YSCore 约定的业务日志作用域扩展。
/// </summary>
public static class YSCoreLoggerExtensions
{
    /// <summary>
    /// 向当前日志作用域添加稳定的业务操作名称和可选的结构化属性。
    /// </summary>
    public static IDisposable? BeginBusinessScope(
        this ILogger logger,
        string operationName,
        IReadOnlyDictionary<string, object?>? properties = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var scope = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [YSCoreObservabilityConstants.OperationNameTag] = operationName
        };

        if (properties != null)
        {
            foreach (var property in properties)
            {
                scope[property.Key] = property.Value;
            }
        }

        return logger.BeginScope(scope);
    }
}
