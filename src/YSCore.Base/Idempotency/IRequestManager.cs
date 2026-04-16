using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Idempotency
{
    /// <summary>
    /// Http请求的幂等性是指无论执行多少次相同的请求，结果都是相同的
    /// </summary>
    public interface IRequestManager
    {
        Task<bool> ExistAsync(Guid id);

        Task CreateRequestForCommandAsync<T>(Guid id);
    }
}
