using Microsoft.AspNetCore.Http;

namespace NexDDD.Core.Exceptions.Extensions
{
    /// <summary>
    /// 异常扩展
    /// 负责友好异常模块中的 AppFriendlyExceptionExtensions 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public static class AppFriendlyExceptionExtensions
    {
        /// <summary>
        /// 设置异常状态码
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static AppFriendlyException StatusCode(this AppFriendlyException exception, int statusCode = StatusCodes.Status500InternalServerError)
        {
            exception.StatusCode = statusCode;
            return exception;
        }

        /// <summary>
        /// 设置额外数据
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static AppFriendlyException WithData(this AppFriendlyException exception, object data)
        {
            exception.Data = data;
            return exception;
        }

        /// <summary>
        /// 设置错误码
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static AppFriendlyException WithErrorCode(this AppFriendlyException exception, object errorCode)
        {
            exception.ErrorCode = errorCode;
            exception.OriginErrorCode ??= errorCode;
            return exception;
        }

        /// <summary>
        /// 设置内部异常是否对外透传
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="exposeInnerException"></param>
        /// <returns></returns>
        public static AppFriendlyException WithExposeInnerException(this AppFriendlyException exception, bool exposeInnerException = true)
        {
            exception.ExposeInnerException = exposeInnerException;
            return exception;
        }
    }
}
