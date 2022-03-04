using System;
using HappyTravel.Edo.Api.Infrastructure;

namespace HappyTravel.Edo.UnitTests.Mocks
{
    public class DateTimeProviderMock : IDateTimeProvider
    {
        public DateTimeProviderMock(DateTimeOffset dateTime)
        {
            _dateTime = dateTime;
        }


        private readonly DateTimeOffset _dateTime;

        public DateTimeOffset UtcNow() => _dateTime;

        public DateTimeOffset UtcTomorrow() => new DateTimeOffset(_dateTime.AddDays(1).Date, TimeSpan.Zero);

        public DateTimeOffset UtcToday() => new DateTimeOffset(_dateTime.Date, TimeSpan.Zero);
    }
}