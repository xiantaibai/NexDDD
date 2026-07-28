namespace NexDDD.Observability.Options;

/// <summary>
/// 定义NexDDD 应用统一接入 OpenTelemetry 时使用的总配置。
/// </summary>
public sealed class NexDddObservabilityOptions
{
    /// <summary>
    /// 获取或设置是否启用 NexDDD 可观测性管线，默认启用。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置服务名称，对应 OpenTelemetry Resource 的 service.name；未配置时使用应用名称。
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// 获取或设置服务版本，对应 OpenTelemetry Resource 的 service.version；未配置时使用入口程序集版本。
    /// </summary>
    public string? ServiceVersion { get; set; }

    /// <summary>
    /// 获取或设置部署环境名称，对应 deployment.environment.name；未配置时使用 Host 环境名称。
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// 获取或设置服务实例标识，对应 service.instance.id；应保证同一服务的运行实例之间可区分。
    /// </summary>
    public string? ServiceInstanceId { get; set; }

    /// <summary>
    /// 获取或设置是否把现有 NexDDD DiagnosticListener 事件桥接到 OpenTelemetry，默认启用。
    /// </summary>
    public bool EnableFrameworkDiagnostics { get; set; } = true;

    /// <summary>
    /// 获取或设置结构化日志采集配置。
    /// </summary>
    public NexDddLoggingOptions Logging { get; set; } = new();

    /// <summary>
    /// 获取或设置分布式链路采集配置。
    /// </summary>
    public NexDddTracingOptions Tracing { get; set; } = new();

    /// <summary>
    /// 获取或设置指标采集配置。
    /// </summary>
    public NexDddMetricsOptions Metrics { get; set; } = new();

    /// <summary>
    /// 获取或设置 OTLP 遥测数据输出配置。
    /// </summary>
    public NexDddOtlpOptions Otlp { get; set; } = new();

    internal void Validate()
    {
        if (Tracing.SamplingRatio is < 0 or > 1)
        {
            throw new InvalidOperationException(
                $"{NexDddObservabilityConstants.ConfigurationSectionName}:Tracing:SamplingRatio must be between 0 and 1.");
        }

        if (Metrics.ExportIntervalMilliseconds <= 0)
        {
            throw new InvalidOperationException(
                $"{NexDddObservabilityConstants.ConfigurationSectionName}:Metrics:ExportIntervalMilliseconds must be greater than 0.");
        }

        if (!string.IsNullOrWhiteSpace(Otlp.Endpoint)
            && (!Uri.TryCreate(Otlp.Endpoint, UriKind.Absolute, out var endpoint)
                || endpoint.Scheme is not ("http" or "https")))
        {
            throw new InvalidOperationException(
                $"{NexDddObservabilityConstants.ConfigurationSectionName}:Otlp:Endpoint must be an absolute HTTP or HTTPS URI.");
        }

        if (!string.Equals(Otlp.Protocol, "grpc", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Otlp.Protocol, "http/protobuf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"{NexDddObservabilityConstants.ConfigurationSectionName}:Otlp:Protocol must be 'grpc' or 'http/protobuf'.");
        }
    }
}

/// <summary>
/// 定义结构化日志采集、日志作用域和格式化消息的配置。
/// </summary>
public sealed class NexDddLoggingOptions
{
    /// <summary>
    /// 获取或设置是否启用 OpenTelemetry 日志采集，默认启用。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置是否采集 ILogger 作用域中的结构化属性，默认启用。
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// 获取或设置是否同时采集格式化后的日志消息，默认启用。
    /// </summary>
    public bool IncludeFormattedMessage { get; set; } = true;

    /// <summary>
    /// 获取或设置是否把日志状态解析为独立结构化属性，默认启用。
    /// </summary>
    public bool ParseStateValues { get; set; } = true;
}

/// <summary>
/// 定义链路采集、采样比例和异常记录的配置。
/// </summary>
public sealed class NexDddTracingOptions
{
    /// <summary>
    /// 获取或设置是否启用分布式链路采集，默认启用。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置链路采样比例，取值范围为 0 到 1，默认全部采样。
    /// </summary>
    public double SamplingRatio { get; set; } = 1;

    /// <summary>
    /// 获取或设置是否把未处理异常记录为链路事件，默认启用；异常消息可能包含敏感信息。
    /// </summary>
    public bool RecordException { get; set; } = true;
}

/// <summary>
/// 定义指标采集是否启用。
/// </summary>
public sealed class NexDddMetricsOptions
{
    /// <summary>
    /// 获取或设置是否启用指标采集，默认启用。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置 OTLP 指标周期导出的间隔毫秒数，默认 60000 毫秒；开发环境可缩短以便快速观察，生产环境不宜设置过小。
    /// </summary>
    public int ExportIntervalMilliseconds { get; set; } = 60_000;
}

/// <summary>
/// 定义通过 OTLP 输出遥测数据时使用的端点、协议和请求头配置。
/// </summary>
public sealed class NexDddOtlpOptions
{
    /// <summary>
    /// 获取或设置是否创建 OTLP Exporter，默认不启用网络输出。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 获取或设置 OTLP Collector 的绝对 HTTP 或 HTTPS 地址；使用 HTTP/Protobuf 时应配置基础地址，框架会按遥测类型补充标准路径。
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// 获取或设置 OTLP 传输协议，可选值为 grpc、http/protobuf 或 HttpProtobuf，默认使用 grpc。
    /// </summary>
    public string Protocol { get; set; } = "grpc";

    /// <summary>
    /// 获取或设置发送到 Collector 的请求头，格式遵循 OpenTelemetry OTLP Exporter 约定；不得在源码中保存密钥。
    /// </summary>
    public string? Headers { get; set; }
}
