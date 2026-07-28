using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.Context;

namespace NexDDD.Core.Idempotency
{
    /// <summary>
    /// 幂等上下文构建
    /// </summary>
    public sealed class IdempotencyExecutionContextFactory : IContextFactory<IdempotencyExecutionContext>
    {
        public IdempotencyExecutionContext? CreateDefault()
        {
            return null;
        }
    }
}
