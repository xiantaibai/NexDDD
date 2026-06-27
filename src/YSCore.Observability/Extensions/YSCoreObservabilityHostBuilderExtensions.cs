using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using YSCore.Observability.Abstractions;
using YSCore.Observability.Internal;
using YSCore.Observability.Options;

namespace YSCore.Observability.Extensions;

/// <summary>
/// 负责在应用 Host 中统一注册 YSCore 日志、链路、指标和 OTLP 输出管线。
/// </summary>
public static class YSCoreObservabilityHostBuilderExtensions
{
    /// <summary>
    /// 注册标准 YSCore 日志、链路、指标、框架诊断桥和 OTLP 输出管线。
    /// </summary>
    public static IHostApplicationBuilder AddYSCoreObservability(
        this IHostApplicationBuilder builder,
        Action<YSCoreObservabilityOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor =>
                descriptor.ServiceType == typeof(YSCoreObservabilityRegistrationMarker)))
        {
            return builder;
        }

        var settings = new YSCoreObservabilityOptions();
        builder.Configuration
            .GetSection(YSCoreObservabilityConstants.ConfigurationSectionName)
            .Bind(settings);
        ApplyOpenTelemetryCompatibilityConfiguration(builder.Configuration, settings);
        configure?.Invoke(settings);
        settings.Otlp.Protocol = NormalizeOtlpProtocol(settings.Otlp.Protocol);

        settings.ServiceName = string.IsNullOrWhiteSpace(settings.ServiceName)
            ? builder.Environment.ApplicationName
            : settings.ServiceName;
        settings.ServiceVersion = string.IsNullOrWhiteSpace(settings.ServiceVersion)
            ? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            : settings.ServiceVersion;
        settings.Environment = string.IsNullOrWhiteSpace(settings.Environment)
            ? builder.Environment.EnvironmentName
            : settings.Environment;
        settings.Validate();

        builder.Services.AddSingleton<YSCoreObservabilityRegistrationMarker>();
        builder.Services.AddSingleton(settings);
        builder.Services.TryAddSingleton<YsObservability>();
        builder.Services.TryAddSingleton<IYsObservability>(
            serviceProvider => serviceProvider.GetRequiredService<YsObservability>());

        if (!settings.Enabled)
        {
            return builder;
        }

        var openTelemetry = builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => ConfigureResource(resource, settings));

        if (settings.Tracing.Enabled)
        {
            openTelemetry.WithTracing(tracing =>
            {
                tracing
                    .SetSampler(
                        new ParentBasedSampler(
                            new TraceIdRatioBasedSampler(settings.Tracing.SamplingRatio)))
                    .AddSource(
                        YSCoreObservabilityConstants.FrameworkActivitySourceName,
                        YSCoreObservabilityConstants.BusinessActivitySourceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = settings.Tracing.RecordException;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = settings.Tracing.RecordException;
                    });

                if (settings.Otlp.Enabled)
                {
                    tracing.AddOtlpExporter(options =>
                        ConfigureOtlp(options, settings.Otlp, "v1/traces"));
                }
            });
        }

        if (settings.Metrics.Enabled)
        {
            openTelemetry.WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(
                        YSCoreObservabilityConstants.FrameworkMeterName,
                        YSCoreObservabilityConstants.BusinessMeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (settings.Otlp.Enabled)
                {
                    metrics.AddOtlpExporter((exporterOptions, readerOptions) =>
                    {
                        ConfigureOtlp(
                            exporterOptions,
                            settings.Otlp,
                            "v1/metrics");
                        readerOptions.PeriodicExportingMetricReaderOptions
                            .ExportIntervalMilliseconds =
                            settings.Metrics.ExportIntervalMilliseconds;
                    });
                }
            });
        }

        if (settings.Logging.Enabled)
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeScopes = settings.Logging.IncludeScopes;
                logging.IncludeFormattedMessage = settings.Logging.IncludeFormattedMessage;
                logging.ParseStateValues = settings.Logging.ParseStateValues;
                logging.SetResourceBuilder(CreateResourceBuilder(settings));

                if (settings.Otlp.Enabled)
                {
                    logging.AddOtlpExporter(options =>
                        ConfigureOtlp(options, settings.Otlp, "v1/logs"));
                }
            });
        }

        if (settings.EnableFrameworkDiagnostics)
        {
            builder.Services.AddHostedService<YSCoreDiagnosticListenerBridge>();
        }

        return builder;
    }

    private static void ConfigureResource(
        ResourceBuilder resource,
        YSCoreObservabilityOptions settings)
    {
        resource.AddService(
            serviceName: settings.ServiceName!,
            serviceVersion: settings.ServiceVersion);

        var attributes = CreateResourceAttributes(settings);
        if (attributes.Count > 0)
        {
            resource.AddAttributes(attributes);
        }
    }

    private static ResourceBuilder CreateResourceBuilder(
        YSCoreObservabilityOptions settings)
    {
        var resource = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName: settings.ServiceName!,
                serviceVersion: settings.ServiceVersion);

        var attributes = CreateResourceAttributes(settings);
        return attributes.Count == 0
            ? resource
            : resource.AddAttributes(attributes);
    }

    private static List<KeyValuePair<string, object>> CreateResourceAttributes(
        YSCoreObservabilityOptions settings)
    {
        var attributes = new List<KeyValuePair<string, object>>();

        if (!string.IsNullOrWhiteSpace(settings.Environment))
        {
            attributes.Add(
                new KeyValuePair<string, object>(
                    "deployment.environment.name",
                    settings.Environment));
        }

        if (!string.IsNullOrWhiteSpace(settings.ServiceInstanceId))
        {
            attributes.Add(
                new KeyValuePair<string, object>(
                    "service.instance.id",
                    settings.ServiceInstanceId));
        }

        return attributes;
    }

    private static void ConfigureOtlp(
        OtlpExporterOptions exporter,
        YSCoreOtlpOptions settings,
        string signalPath)
    {
        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            exporter.Endpoint = CreateOtlpEndpoint(
                settings.Endpoint,
                settings.Protocol,
                signalPath);
        }

        exporter.Protocol = string.Equals(
            settings.Protocol,
            "http/protobuf",
            StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;

        if (!string.IsNullOrWhiteSpace(settings.Headers))
        {
            exporter.Headers = settings.Headers;
        }
    }

    private static void ApplyOpenTelemetryCompatibilityConfiguration(
        IConfiguration configuration,
        YSCoreObservabilityOptions settings)
    {
        var section = configuration.GetSection("OpenTelemetry");
        var endpoint = section["OtlpExporterEndpoint"];
        var protocol = section["Protocol"];

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            settings.Otlp.Endpoint = endpoint;
            settings.Otlp.Enabled = true;
        }

        if (!string.IsNullOrWhiteSpace(protocol))
        {
            settings.Otlp.Protocol = protocol;
        }
    }

    private static string NormalizeOtlpProtocol(string protocol)
    {
        return protocol.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("/", string.Empty, StringComparison.Ordinal)
            .Equals("httpprotobuf", StringComparison.OrdinalIgnoreCase)
            ? "http/protobuf"
            : protocol.Equals("grpc", StringComparison.OrdinalIgnoreCase)
                ? "grpc"
                : protocol;
    }

    private static Uri CreateOtlpEndpoint(
        string endpoint,
        string protocol,
        string signalPath)
    {
        var endpointUri = new Uri(endpoint);
        if (!string.Equals(
                protocol,
                "http/protobuf",
                StringComparison.OrdinalIgnoreCase))
        {
            return endpointUri;
        }

        var expectedPath = $"/{signalPath}";
        if (endpointUri.AbsolutePath.EndsWith(
                expectedPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return endpointUri;
        }

        var uriBuilder = new UriBuilder(endpointUri)
        {
            Path = $"{endpointUri.AbsolutePath.TrimEnd('/')}{expectedPath}"
        };
        return uriBuilder.Uri;
    }

    /// <summary>
    /// 标记当前服务集合已经完成可观测性注册，防止重复添加遥测管线。
    /// </summary>
    private sealed class YSCoreObservabilityRegistrationMarker
    {
    }
}
