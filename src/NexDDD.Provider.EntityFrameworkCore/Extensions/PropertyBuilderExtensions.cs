using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Provider.EntityFrameworkCore.ValueGenerators;
using NexDDD.Core.TimeZone;

namespace NexDDD.Provider.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// 提供EF Core 持久化模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
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
            // return builder.HasValueGenerator((p, e) => new ClockDateTimeValueGenerator(clock));
            return builder.HasValueGenerator<ClockDateTimeValueGenerator>();
        }

        public static PropertyBuilder<DateTime?> UseClockDateTimeValueGenerator(
        this PropertyBuilder<DateTime?> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
            return builder.HasValueGenerator<ClockDateTimeValueGenerator>();
            // return builder.HasValueGenerator((p, e) => new ClockDateTimeValueGenerator(clock));
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
