using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Idempotency
{
    /// <summary>
    /// 负责幂等执行模块中的 ClientRequest 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class ClientRequest
    {
        public Guid Id { get; set; }
        [Required]
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public string Name { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public DateTime Time { get; set; }
    }
}
