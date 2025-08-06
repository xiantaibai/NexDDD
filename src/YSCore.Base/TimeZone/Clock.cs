using Microsoft.Extensions.Options;

namespace YSCore.Base.TimeZone
{
    public class Clock : IClock
    {
        protected ClockOptions Options { get; }
        private readonly TimeZoneResolver _timeZoneResolver;
        public Clock(IOptions<ClockOptions> options, TimeZoneResolver timeZoneResolver)
        {
            Options = options.Value;
            _timeZoneResolver = timeZoneResolver;
        }

        public virtual DateTime Now => Options.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;

        public virtual DateTimeKind Kind => Options.Kind;

        public virtual bool SupportsMultipleTimezone => Options.Kind == DateTimeKind.Utc;

        //public virtual DateTime Normalize(DateTime dateTime)
        //{
        //    if (Kind == DateTimeKind.Unspecified || Kind == dateTime.Kind)
        //    {
        //        return dateTime;
        //    }

        //    if (Kind == DateTimeKind.Local && dateTime.Kind == DateTimeKind.Utc)
        //    {
        //        return dateTime.ToLocalTime();
        //    }

        //    if (Kind == DateTimeKind.Utc && dateTime.Kind == DateTimeKind.Local)
        //    {
        //        return dateTime.ToUniversalTime();
        //    }

        //    return DateTime.SpecifyKind(dateTime, Kind);
        //}
        public virtual DateTime Normalize(DateTime dateTime)
        {
            if (Kind == DateTimeKind.Unspecified || Kind == dateTime.Kind)
            {
                return DateTime.SpecifyKind(dateTime, Kind);
            }

            var timeZone = _timeZoneResolver.GetTimeZone();
            if (Kind == DateTimeKind.Local && dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);
            }

            if (Kind == DateTimeKind.Utc && dateTime.Kind == DateTimeKind.Local)
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
            }

            return DateTime.SpecifyKind(dateTime, Kind);
        }
    }
}
