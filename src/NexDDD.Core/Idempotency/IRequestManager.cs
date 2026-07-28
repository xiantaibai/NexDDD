using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Idempotency
{
    /// <summary>
    /// Http请求的幂等性是指无论执行多少次相同的请求，结果都是相同的
    /// 定义幂等执行模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IRequestManager
    {
        Task<bool> ExistAsync(Guid id);

        Task CreateRequestForCommandAsync<T>(Guid id);
    }
}
