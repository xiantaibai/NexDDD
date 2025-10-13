using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.TimeZone;

namespace YSCore.Provider.EntityFrameworkCore.ValueGenerators
{
    public class ClockDateTimeValueGenerator : ValueGenerator<DateTime>
    {
        public override DateTime Next(EntityEntry entry)
        {
            var clock = entry.Context.GetService<IClock>();
            return clock.Now;
        }

        public override bool GeneratesTemporaryValues => false;
    }
}
