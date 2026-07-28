using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Idempotency
{
    /// <summary>
    /// 幂等上下文，包含幂等请求的相关信息，如类型码、键哈希和请求ID等。这些信息用于识别和处理重复请求，以确保幂等性。
    /// </summary>
    public sealed class IdempotencyExecutionContext
    {
        public string TypeCode { get; set; } = default!;

        public string KeyHash { get; set; } = default!;

        public string RequestId { get; set; } = default!;
    }
}
