using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Integration
{
    /// <summary>
    /// 负责集成事件模块中的 IntergrationEventSubscriptionInfo 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class IntergrationEventSubscriptionInfo
    {
        public Dictionary<string, Type> EventTypes { get; } = [];
    }
}
