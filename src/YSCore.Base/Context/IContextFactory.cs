using System;

namespace YSCore.Base.Context
{
    /// <summary>
    /// 工厂创建
    /// </summary>
    public interface IContextFactory<TContext> where TContext : class
    {
        TContext CreateDefault();
    }

    public class DefaultContextFactory<TContext> : IContextFactory<TContext> where TContext : class
    {
        public TContext CreateDefault()
        {
            return Activator.CreateInstance<TContext>() ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TContext).Name}. Ensure it has a parameterless constructor or provide a custom factory.");
        }
    }
}
