namespace YSCore.Observability;

/// <summary>
/// 集中维护 YSCore 可观测性的数据源名称、指标源名称和语义属性名称。
/// </summary>
public static class YSCoreObservabilityConstants
{
    public const string ConfigurationSectionName = "YSCore:Observability";

    public const string FrameworkActivitySourceName = "YSCore.Framework";

    public const string BusinessActivitySourceName = "YSCore.Business";

    public const string FrameworkMeterName = "YSCore.Framework";

    public const string BusinessMeterName = "YSCore.Business";

    public const string OperationNameTag = "yscore.operation.name";

    public const string OperationCategoryTag = "yscore.operation.category";

    public const string OperationStatusTag = "yscore.operation.status";
}
