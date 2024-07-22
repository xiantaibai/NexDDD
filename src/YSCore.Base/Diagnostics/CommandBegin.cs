using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Diagnostics
{
    public record CommandBegin(Guid Id, string Name, object CommandData);
}
