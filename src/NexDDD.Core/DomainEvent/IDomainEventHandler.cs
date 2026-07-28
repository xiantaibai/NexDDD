using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DomainEvent
{
    /// <summary>
    /// 定义领域事件模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
    }
}
