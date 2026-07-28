using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Provider.EntityFrameworkCore.ValueGenerators
{
    /// <summary>
    /// 负责EF Core 持久化模块中的 LocalDateTimeGenerator 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class LocalDateTimeGenerator : ValueGenerator<DateTime>
    {
        public override DateTime Next(EntityEntry entry)
        {
            return DateTime.Now;
        }

        public override bool GeneratesTemporaryValues => false;
    }
}
