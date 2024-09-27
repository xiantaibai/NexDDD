using DotNetCore.CAP.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Provider.Cap
{
    public class CapFilter: SubscribeFilter
    {
        public override Task OnSubscribeExecutingAsync(ExecutingContext context)
        {
            var methodInfo = context.ConsumerDescriptor.MethodInfo;

            // 订阅方法执行前
            var message = context.DeliverMessage;
            var parameterDescriptors = context.ConsumerDescriptor.Parameters;
            var executeParameters = new object[parameterDescriptors.Count];


        }
    }
}
