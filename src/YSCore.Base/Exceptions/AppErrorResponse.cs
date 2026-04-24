namespace YSCore.Base.Exceptions
{
    /// <summary>
    /// 返回给客户端的精简异常对象。
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
