using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Diagnostics
{
    /// <summary>
    /// 承载DomainEventHandlerBegin 诊断事件的数据快照，供 DiagnosticListener 发布并由可观测性组件消费。
    /// </summary>
    public record DomainEventHandlerBegin(Guid Id, string Name, object EventData);
}
