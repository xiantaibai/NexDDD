using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Diagnostics
{
    public record CommandEnd(Guid Id, string Name, object CommandData);
}
