namespace NexDDD.Core.Exceptions
{
    /// <summary>
    /// 统一异常详情对象。
    /// 负责友好异常模块中的 AppErrorInfo 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public sealed class AppErrorInfo
    {
        /// <summary>
        /// 错误码。
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 原始错误码。
        /// </summary>
        public string OriginCode { get; set; }

        /// <summary>
        /// 解析后的错误消息。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// HTTP 状态码。
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 是否为校验异常。
        /// </summary>
        public bool ValidationException { get; set; }

        /// <summary>
        /// OpenTelemetry 跨服务链路标识；没有有效 Activity 时回退到 HTTP 请求标识。
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// CLR 异常类型。
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// 附加数据。
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 内部异常详情。
        /// </summary>
        public AppErrorInfo InnerError { get; set; }
    }
}
