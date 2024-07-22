using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.DomainEvent
{
    /// <summary>
    /// 表示一个领域事件
    /// </summary>
    public interface IDomainEvent : INotification
    {
    }
}
