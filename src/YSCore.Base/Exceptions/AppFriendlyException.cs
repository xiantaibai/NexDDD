using Microsoft.AspNetCore.Http;
using System;
using System.Runtime.Serialization;

namespace YSCore.Base.Exceptions
{
    /// <summary>
    /// 友好异常类型。
    /// </summary>
    public class AppFriendlyException : Exception
    {
        /// <summary>
        /// 创建友好异常。
        /// </summary>
        public AppFriendlyException()
        {
        }

        /// <summary>
        /// 创建带错误消息的友好异常。
        /// </summary>
        public AppFriendlyException(string message) : base(message)
        {
            ErrorMessage = message;
        }

        /// <summary>
        /// 创建带错误消息和错误码的友好异常。
        /// </summary>
        public AppFriendlyException(string message, object errorCode) : base(message)
        {
            ErrorMessage = message;
            ErrorCode = OriginErrorCode = errorCode;
        }

        /// <summary>
        /// 创建带错误消息和内部异常的友好异常。
        /// </summary>
        public AppFriendlyException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorMessage = message;
        }

        /// <summary>
        /// 创建带错误消息、错误码和内部异常的友好异常。
        /// </summary>
        public AppFriendlyException(string message, object errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorMessage = message;
            ErrorCode = OriginErrorCode = errorCode;
        }

        /// <summary>
        /// 序列化构造函数。
        /// </summary>
        public AppFriendlyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// 错误码。
        /// </summary>
        public object ErrorCode { get; set; }

        /// <summary>
        /// 覆盖前的原始错误码。
        /// </summary>
        public object OriginErrorCode { get; set; }

        /// <summary>
        /// 错误消息。
        /// </summary>
        public object ErrorMessage { get; set; }

        /// <summary>
        /// 请求状态码。
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;

        /// <summary>
        /// 内部异常是否允许对外暴露。
        /// </summary>
        public bool ExposeInnerException { get; set; } = true;

        /// <summary>
        /// 是否为校验异常。
        /// </summary>
        public bool ValidationException { get; set; }

        /// <summary>
        /// 额外携带的数据。
        /// </summary>
        public new object Data { get; set; }
    }
}
