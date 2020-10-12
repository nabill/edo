using System;
using HappyTravel.Edo.Api.Infrastructure;

namespace HappyTravel.Edo.UnitTests.Mocks
{
    public class DateTimeProviderMock : IDateTimeProvider
    {
        public DateTimeProviderMock(DateTime dateTime)
        {
            _dateTime = dateTime;
        }


        private readonly DateTime _dateTime;

        public DateTime UtcNow() => _dateTime;

        public DateTime UtcTomorrow() => _dateTime.AddDays(1).Date;

        public DateTime UtcToday() => _dateTime.Date;
    }
}