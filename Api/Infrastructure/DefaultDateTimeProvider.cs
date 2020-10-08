using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;

        public DateTime UtcTomorrow() => DateTime.UtcNow.AddDays(1).Date;
    }
}