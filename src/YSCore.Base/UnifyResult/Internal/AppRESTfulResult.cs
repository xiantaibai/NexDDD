
using YSCore.Base.Exceptions;

namespace YSCore.Base.UnifyResult.Internal
{
    /// <summary>
    /// 定义统一 RESTful 响应结构，负责承载业务数据、错误信息和跨服务链路标识。
    /// </summary>
    public class AppRESTfulResult<T>
    {
        /// <summary>
        /// HTTP 状态码。
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// 业务响应数据。
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 是否执行成功。
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 错误信息。
        /// </summary>
        public object Errors { get; set; }

        /// <summary>
        /// 统一响应附加数据。
        /// </summary>
        public object Extras { get; set; }

        /// <summary>
        /// Unix 时间戳，单位为秒。
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// OpenTelemetry 跨服务链路标识；没有有效 Activity 时回退到 HTTP 请求标识。
        /// </summary>
        public string TraceId { get; set; }


        internal static AppRESTfulResult<T> Failed(List<AppFriendlyException>? errors)
        {
            var result = new AppRESTfulResult<T> { Succeeded = false };
            if (errors != null)
            {
                result.Errors = errors;
            }
            return result;
        }
    }
}
