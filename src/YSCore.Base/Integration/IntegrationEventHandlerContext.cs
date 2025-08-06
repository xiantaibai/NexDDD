using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Integration
{
    /// <summary>
    /// 集成事件处理程序上下文
    /// </summary>
    public class IntegrationEventHandlerContext
    {
        /// <summary>
        /// 事件源（事件承载对象）
        /// </summary>
        public IIntegrationEvent Source { get; set; }

        /// <summary>
        /// 共享上下文数据
        /// </summary>
        public IDictionary<object, object> Properties { get; set; }

        public IntegrationEventHandlerContext(IIntegrationEvent source, IDictionary<object, object> properties)
        {
            Source = source;
            Properties = properties;
        }
    }
}
