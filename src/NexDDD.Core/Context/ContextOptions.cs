using System;

namespace NexDDD.Core.Context
{
    /// <summary>
    /// 配置信息
    /// 负责请求上下文模块中的 ContextOptions 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ContextOptions<TContext> where TContext : class
    {
        /// <summary>
        /// 获取或设置上下文工厂，用于在当前上下文不存在时创建默认实例。
        /// </summary>
        public IContextFactory<TContext> ContextFactory { get; set; }

        /// <summary>
        /// 获取或设置上下文序列化器，用于跨调用边界传递上下文数据。
        /// </summary>
        public IContextSerializer Serializer { get; set; }
    }
}
