using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Provider.EntityFrameworkCore.ValueGenerators;

namespace YSCore.Provider.EntityFrameworkCore.Extensions
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<DateTime> UseUtcDateTimeValueGenerator(
        this PropertyBuilder<DateTime> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));
           
            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<UtcDateTimeGenerator>();
        }

        public static PropertyBuilder<DateTime?> UseUtcDateTimeValueGenerator(
        this PropertyBuilder<DateTime?> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<UtcDateTimeGenerator>();
        }

        public static PropertyBuilder<DateTime> UseLocalDateTimeValueGenerator(
        this PropertyBuilder<DateTime> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<LocalDateTimeGenerator>();
        }

        public static PropertyBuilder<DateTime?> UseLocalDateTimeValueGenerator(
        this PropertyBuilder<DateTime?> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<LocalDateTimeGenerator>();
        }

        public static PropertyBuilder<DateTime> UseClockDateTimeValueGenerator(
       this PropertyBuilder<DateTime> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<ClockDateTimeValueGenerator>();
        }

        public static PropertyBuilder<DateTime?> UseClockDateTimeValueGenerator(
        this PropertyBuilder<DateTime?> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<ClockDateTimeValueGenerator>();
        }

        public static PropertyBuilder<long> UseSnowFlakeValueGenerator(
            this PropertyBuilder<long> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.HasValueGenerator<IdGenerator>();
        }

        public static PropertyBuilder<long?> UseSnowFlakeValueGenerator(
            this PropertyBuilder<long?> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.HasValueGenerator<IdGenerator>();
        }
    }
}
