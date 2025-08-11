using System;

namespace YSCore.Base.Context
{
    /// <summary>
    /// 配置信息
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ContextOptions<TContext> where TContext : class
    {
        public IContextFactory<TContext> ContextFactory { get; set; }
        public IContextSerializer Serializer { get; set; }
    }
}
