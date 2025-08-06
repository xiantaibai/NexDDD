using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        private readonly IClock _clock;
        public ClockDateTimeValueGenerator(IClock clock)
        {
            _clock = clock;
        }

        public override DateTime Next(EntityEntry entry)
        {
            return _clock.Now;
        }

        public override bool GeneratesTemporaryValues => false;
    }
}
