using System;

namespace NexDDD.Core.Context
{
    /// <summary>
    /// 工厂创建
    /// </summary>
    public interface IContextFactory<TContext> where TContext : class
    {
        TContext CreateDefault();
    }

    /// <summary>
    /// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class DefaultContextFactory<TContext> : IContextFactory<TContext> where TContext : class
    {
        public TContext CreateDefault()
        {
            return Activator.CreateInstance<TContext>() ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TContext).Name}. Ensure it has a parameterless constructor or provide a custom factory.");
        }
    }
}
