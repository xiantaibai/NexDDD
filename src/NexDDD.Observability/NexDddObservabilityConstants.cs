namespace NexDDD.Observability;

/// <summary>
/// 集中维护 NexDDD 可观测性的数据源名称、指标源名称和语义属性名称。
/// 负责可观测性模块中的 NexDddObservabilityConstants 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
public static class NexDddObservabilityConstants
{
    public const string ConfigurationSectionName = "NexDDD:Observability";

    public const string FrameworkActivitySourceName = "NexDDD.Framework";

    public const string BusinessActivitySourceName = "NexDDD.Business";

    public const string FrameworkMeterName = "NexDDD.Framework";

    public const string BusinessMeterName = "NexDDD.Business";

    public const string OperationNameTag = "nexddd.operation.name";

    public const string OperationCategoryTag = "nexddd.operation.category";

    public const string OperationStatusTag = "nexddd.operation.status";
}
