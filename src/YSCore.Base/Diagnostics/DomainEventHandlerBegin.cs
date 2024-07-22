using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Diagnostics
{
    public record DomainEventHandlerBegin(Guid Id, string Name, object EventData);
}
