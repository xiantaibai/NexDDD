using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Integration
{
    public class IntergrationEventSubscriptionInfo
    {
        public Dictionary<string, Type> EventTypes { get; } = [];
    }
}
