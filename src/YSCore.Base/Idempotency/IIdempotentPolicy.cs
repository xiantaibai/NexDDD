using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Idempotency
{
    /// <summary>
    /// 幂等指令策略，用于标识一个指令是否幂等，并提供创建重复结果的方法。
    /// </summary>
    public interface IIdempotentPolicy<TResponse>
    {
        /// 是否启用幂等
        bool IsIdempotent => true;

        /// 是否匹配当前异常（用于精确控制）
        bool Match(Exception ex);

        /// 幂等命中时返回结果
        TResponse OnDuplicate();
    }
}
