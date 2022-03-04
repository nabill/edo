using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;

        public DateTimeOffset UtcTomorrow() => new (DateTimeOffset.UtcNow.AddDays(1).Date, TimeSpan.Zero);

        public DateTimeOffset UtcToday() => new (DateTime.UtcNow.Date, TimeSpan.Zero);
    }
}