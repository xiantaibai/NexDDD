using Microsoft.Extensions.Configuration;
using YSCore.Base.Idempotency;

namespace YSCore.Base.Exceptions.Options
{
    public class FriendlyExceptionSettingsOptions
    {
        /// <summary>
        /// 隐藏错误码
        /// </summary>
        public bool? HideErrorCode { get; set; }

        /// <summary>
        /// 默认错误码
        /// </summary>
        public string DefaultErrorCode { get; set; }

        /// <summary>
        /// 默认错误消息
        /// </summary>
        public string DefaultErrorMessage { get; set; }

        /// <summary>
        /// 错误码消息映射
        /// </summary>
        public Dictionary<string, string> ErrorCodeMessages { get; set; }

        /// <summary>
        /// 标记 Oops.Oh 为业务异常
        /// </summary>
        /// <remarks>也就是不会进入异常处理</remarks>
        public bool? ThrowBah { get; set; }

        /// <summary>
        /// 是否向外透传内部异常
        /// </summary>
        public bool? PropagateInnerException { get; set; }

        /// <summary>
        /// 是否输出异常日志
        /// </summary>
        public bool? LogError { get; set; }

        /// <summary>
        /// 选项后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(FriendlyExceptionSettingsOptions options, IConfiguration configuration)
        {
            options.HideErrorCode ??= false;
            options.DefaultErrorCode ??= string.Empty;
            options.DefaultErrorMessage ??= "服务异常";
            options.ErrorCodeMessages ??= new Dictionary<string, string>();
            options.ErrorCodeMessages.TryAdd(IdempotencyErrorCodes.RepeatSubmit, "请勿重复提交相同请求。");
            options.ErrorCodeMessages.TryAdd(IdempotencyErrorCodes.CommandLocked, "当前请求正在处理中，请稍后再试。");
            options.ThrowBah ??= false;
            options.PropagateInnerException ??= true;
            options.LogError ??= true;
        }
    }
}
