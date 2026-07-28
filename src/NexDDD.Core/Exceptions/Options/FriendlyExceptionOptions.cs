namespace NexDDD.Core.Exceptions.Options
{
    /// <summary>
    /// 承载友好异常模块的运行配置，供依赖注入注册过程与运行时组件读取。
    /// </summary>
    public sealed class FriendlyExceptionOptions
    {
        /// <summary>
        /// 是否启用全局友好异常
        /// </summary>
        public bool GlobalEnabled { get; set; } = true;
    }
}
