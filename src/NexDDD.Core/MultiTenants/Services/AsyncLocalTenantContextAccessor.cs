using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.MultiTenants
{
    /// <summary>
    /// 提供多租户模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class AsyncLocalTenantContextAccessor<T> : ITenantContextAccessor<T> where T : Tenant
    {
        /// <summary>
        /// 保存当前异步调用链的租户上下文入口。
        /// </summary>
        private static readonly AsyncLocal<TenantInfoHolder> asyncLocalContext = new();

        /// <summary>
        /// 获取或设置当前异步调用链的租户上下文。
        /// </summary>
        public T? TenantInfo
        {
            get
            {
                if (asyncLocalContext?.Value == null)
                    return default;
                return asyncLocalContext.Value.Context;
            }
            set
            {
                // 替换租户前先清空旧持有者，避免上下文继续被异步分支引用。
                var holder = asyncLocalContext.Value;
                if (holder != null)
                    holder.Context = default;

                // 为当前异步调用链设置新的租户上下文。
                if (value != null)
                    asyncLocalContext.Value = new TenantInfoHolder { Context = value };
            }
        }

        /// <summary>
        /// 通过对象间接层保存租户上下文，使已捕获的 ExecutionContext 能观察到清理操作。
        /// </summary>
        /// <remarks>
        /// 实现思路参考：https://github.com/aspnet/HttpAbstractions/pull/1066
        /// </remarks>
        private class TenantInfoHolder()
        {
            public T? Context;
        }
    }
}
