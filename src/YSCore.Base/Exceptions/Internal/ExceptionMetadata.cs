using YSCore.Base.Exceptions;

namespace YSCore.Base.Exceptions.Internal
{
    /// <summary>
    /// 异常元数据。
    /// </summary>
    public sealed class ExceptionMetadata
    {
        /// <summary>
        /// HTTP 状态码。
        /// </summary>
        public int StatusCode { get; internal set; }

        /// <summary>
        /// 错误码。
        /// </summary>
        public object ErrorCode { get; internal set; }

        /// <summary>
        /// 原始错误码。
        /// </summary>
        public object OriginErrorCode { get; internal set; }

        /// <summary>
        /// 异常输出内容。
        /// </summary>
        public object Errors { get; internal set; }

        /// <summary>
        /// 附加数据。
        /// </summary>
        public object Data { get; internal set; }

        /// <summary>
        /// 异常实例。
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// 内部详细异常对象。
        /// </summary>
        public AppErrorInfo Error { get; internal set; }

        /// <summary>
        /// 对外返回的精简异常对象。
        /// </summary>
        public AppErrorResponse PublicError { get; internal set; }

        /// <summary>
        /// OpenTelemetry 跨服务链路标识；没有有效 Activity 时回退到 HTTP 请求标识。
        /// </summary>
        public string TraceId { get; internal set; }
    }
}
