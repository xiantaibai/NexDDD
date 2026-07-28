using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;

namespace NexDDD.Core.Context
{
    /// <summary>
    /// 上下文 访问器
    /// 定义请求上下文模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IContextAccessor<TContext> where TContext : class
    {
        TContext Current { get; set; }
        bool HasContext { get; }
        IDisposable SetContext(TContext context);

        /// <summary>
        /// 序列化当前 上下文
        /// </summary>
        /// <returns></returns>
        string SerializeContext();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        TContext DeserializeContext(string json);
    }

    /// <summary>
    /// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class ContextAccessor<TContext> : IContextAccessor<TContext> where TContext : class
    {
        private readonly AsyncLocal<TContext> _currentContext = new AsyncLocal<TContext>();
        private readonly IContextFactory<TContext> _factory;
        private readonly IContextSerializer _serializer;
        private readonly ILogger<ContextAccessor<TContext>> _logger;

        public ContextAccessor(IContextFactory<TContext> factory, IContextSerializer serializer, ILogger<ContextAccessor<TContext>> logger = null)
        {
            _factory = factory;
            _serializer = serializer;
            _logger = logger;
        }

        public bool HasContext => this.Current != null;

        public TContext Current
        {
            get => _currentContext.Value ?? _factory.CreateDefault();
            set
            {
                _currentContext.Value = value;
                _logger?.LogInformation("Context updated: {ContextType}, {ContextValue}", typeof(TContext).Name, _serializer.Serialize(value));
            }
        }

        public IDisposable SetContext(TContext context)
        {
            var previous = _currentContext.Value;
            Current = context;
            return new DisposableContext(this, previous);
        }

        public string SerializeContext()
        {
            return _serializer.Serialize(this.Current);
        }

        public TContext DeserializeContext(string json)
        {
            return _serializer.Deserialize<TContext>(json) ?? _factory.CreateDefault();
        }

        /// <summary>
        /// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
        /// </summary>
        private class DisposableContext : IDisposable
        {
            private readonly IContextAccessor<TContext> _accessor;
            private readonly TContext _previous;

            public DisposableContext(IContextAccessor<TContext> accessor, TContext previous)
            {
                _accessor = accessor;
                _previous = previous;
            }

            public void Dispose() => _accessor.Current = _previous;
        }
    }
}
