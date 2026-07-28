# NexDDD.Observability

`NexDDD.Observability` 是 NexDDD 生态的可观测性门面与统一规范执行器。它同时服务于框架自身和业务系统：负责生成、关联、规范和输出 Logs、Traces、Metrics，但不绑定任何监控存储或可视化产品。

## 职责

### 统一接入

- 通过 `AddNexDddObservability()` 完成 OpenTelemetry 注册。
- 统一 `service.name`、版本、环境和实例等 Resource 信息。
- 自动接入 ASP.NET Core、`HttpClient`、.NET Runtime。
- 使用 OTLP 将遥测数据发送给 OpenTelemetry Collector。
- Collector、Grafana、Prometheus、Tempo、Loki、Jaeger 等属于部署侧选择，不进入业务代码。

### 框架监控

- 桥接现有 NexDDD `DiagnosticListener`。
- 采集 Command、Domain Event、UnitOfWork transaction 和 Redis distributed lock 的次数、耗时和异常。
- 保持旧诊断事件兼容，后续框架组件可以逐步迁移到原生 `ActivitySource` 和 `Meter`。
- Integration Event/CAP 的 W3C Trace Context 传播属于后续重点能力。

### 业务系统能力

- 日志继续使用标准 `ILogger<T>`，自动与当前 TraceId、SpanId 关联。
- `BeginBusinessScope()` 为结构化日志增加稳定的业务操作名和业务属性。
- `INexObservability.StartOperation()` 同时生成业务 Span、执行次数、耗时和错误指标。
- 领域专属指标继续使用标准 `IMeterFactory` 定义强类型 Counter、Histogram、Gauge。

### 统一 Trace Context

- 业务代码和框架组件通过 `NexDddTraceContext` 获取当前 TraceId 和 SpanId。
- TraceId 优先来自 `Activity.Current.TraceId`，用于跨服务链路查询。
- 没有有效 Activity 时，TraceId 回退到 `HttpContext.TraceIdentifier`，确保未启用可观测性的旧应用保持兼容。
- `HttpContext.TraceIdentifier` 不会被覆盖，也不作为独立 RequestId 暴露到统一响应。
- 幂等模块中的 `RequestId` 表示幂等执行所有权，不属于 HTTP 链路标识。

### 统一规则

- Metric 名称、单位、Tag 和 Trace 属性遵循 OpenTelemetry semantic conventions。
- Metric Tag 必须是低基数值；订单号、用户 ID、流水号等只能进入 Log 或 Trace。
- 默认不采集 Command、Event、HTTP Body 的完整内容。
- 密码、Token、手机号等敏感字段由业务日志规范和后续脱敏扩展处理。
- 遥测采集或发送失败不得中断业务流程。
- Trace Sampling 在应用侧配置；生产环境建议由 Collector 配合 Tail Sampling 保留错误链路。

## 快速开始

在入口项目引用本项目，然后注册：

```csharp
using NexDDD.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexDddObservability();
```

配置：

```json
{
  "NexDDD": {
    "Observability": {
      "Enabled": true,
      "ServiceName": "OrderService",
      "Environment": "Production",
      "Tracing": {
        "Enabled": true,
        "SamplingRatio": 0.1,
        "RecordException": true
      },
      "Metrics": {
        "Enabled": true,
        "ExportIntervalMilliseconds": 60000
      },
      "Logging": {
        "Enabled": true,
        "IncludeScopes": true,
        "IncludeFormattedMessage": true,
        "ParseStateValues": true
      },
      "Otlp": {
        "Enabled": true,
        "Endpoint": "http://otel-collector:4317",
        "Protocol": "grpc"
      }
    }
  }
}
```

也可以使用标准环境变量覆盖 OTLP 配置，例如 `OTEL_EXPORTER_OTLP_ENDPOINT`。当 `Otlp.Enabled` 为 `false` 时不会创建网络 Exporter，业务应用仍可安全运行。

为了兼容 Aspire 风格的简化接入，也可以直接配置顶层 `OpenTelemetry` 节点。配置基础地址后会自动启用 OTLP Exporter；使用 `HttpProtobuf` 时，框架会分别补充 `/v1/logs`、`/v1/traces` 和 `/v1/metrics` 标准路径：

```json
{
  "OpenTelemetry": {
    "OtlpExporterEndpoint": "http://otel-collector:4318",
    "Protocol": "HttpProtobuf"
  }
}
```

`test/ObservabilityDemoApi` 提供以下验证入口：

- `GET /observability/success`：输出成功日志、嵌套业务 Span、执行次数和耗时指标。
- `GET /observability/error`：输出异常日志、错误 Span 和错误次数指标。
- `GET /observability/logs`：输出带业务作用域的结构化日志。
- `GET /observability/metrics?count=10`：通过独立 `IMeterFactory` 批量生成 Counter、UpDownCounter、Histogram 和 ObservableGauge 数据，不创建业务 Trace。
- `GET /observability/http-client`：生成入口 HTTP、客户端 HTTP 和下游 HTTP 的关联链路。

Demo 还注册了 `ObservabilityDemoMetricWorker`，即使没有 HTTP 请求和业务 Trace，也会每五秒产生一次后台业务指标。Demo 将 `Metrics:ExportIntervalMilliseconds` 设置为五秒，便于快速观察；生产环境应根据采集规模适当增大导出间隔。

## 业务日志

不创建新的日志抽象，直接使用 `ILogger<T>`：

```csharp
logger.LogInformation(
    "Order {OrderId} confirmed with amount {Amount}",
    order.Id,
    order.Amount);
```

需要增加一组结构化上下文时：

```csharp
using var scope = logger.BeginBusinessScope(
    "order.confirm",
    new Dictionary<string, object?>
    {
        ["order.channel"] = order.Channel
    });
```

`OrderId` 这类高基数业务标识可以进入日志，但不能作为 Metric Tag。

## 业务 Operation

```csharp
/// <summary>
/// 负责演示订单确认流程中的业务链路、结构化日志和操作指标记录。
/// </summary>
public sealed class OrderService(
    INexObservability observability,
    ILogger<OrderService> logger)
{
    public async Task ConfirmAsync(Order order, CancellationToken cancellationToken)
    {
        using var operation = observability.StartOperation(
            "order.confirm",
            tags:
            [
                new("order.channel", order.Channel)
            ]);

        try
        {
            logger.LogInformation("Confirming order {OrderId}", order.Id);
            await ConfirmCoreAsync(order, cancellationToken);
            operation.SetSuccess();
        }
        catch (Exception exception)
        {
            operation.SetError(exception);
            throw;
        }
    }
}
```

稳定的 `operationName` 会成为 Metric Tag。禁止把订单号等动态值拼入操作名，例如不要使用 `order.confirm.123456`。

业务操作结束前应明确调用 `SetSuccess()` 或 `SetError(exception)`；如果没有设置结果，指标状态记录为 `unset`，避免异常路径被误报成成功。

## 分层边界

- `Domain` 不依赖 `NexDDD.Observability`，领域模型只表达业务规则和 Domain Event。
- `Application` 可以使用 `ILogger<T>`、`INexObservability` 和业务 Metric。
- `Infrastructure` 负责数据库、Redis、CAP、HTTP 等技术信号。
- `WebApi`/Host 负责调用 `AddNexDddObservability()` 完成组合。

## 非职责

- 不存储日志、Trace 或 Metric。
- 不实现 Dashboard 和告警执行引擎。
- 不强制使用 Serilog 或指定监控厂商。
- 不自动记录完整业务对象、Command 或消息 Payload。
- 不把可观测性代码放入 Aggregate Root。

## 当前版本与后续计划

当前第一版包含：

- 统一 Resource 和 OTLP 配置。
- Logs、Traces、Metrics 注册。
- ASP.NET Core、`HttpClient`、Runtime 自动采集。
- NexDDD DiagnosticListener 兼容桥接。
- 业务 Operation 和日志 Scope API。

后续计划：

1. CAP Integration Event 的 `traceparent`、`tracestate`、`baggage` 注入与提取。
2. EF Core、Redis Client 等 Provider 的原生 Instrumentation。
3. 日志字段脱敏、Tag 基数保护与扩展 Processor。
4. Collector 示例、Grafana Dashboard 和告警规则。
5. 用原生 `ActivitySource`/`Meter` 逐步替换框架旧 `DiagnosticListener` 埋点。
