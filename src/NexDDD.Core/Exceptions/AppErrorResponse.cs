namespace NexDDD.Core.Exceptions
{
    /// <summary>
    /// 返回给客户端的精简异常对象。
    /// 负责友好异常模块中的 AppErrorResponse 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public sealed class AppErrorResponse
    {
        /// <summary>
        /// 错误码。
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 错误消息。
        /// </summary>
        public string Message { get; set; }
    }
}
