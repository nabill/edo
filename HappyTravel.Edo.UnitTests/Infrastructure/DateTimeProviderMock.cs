using System;
using HappyTravel.Edo.Api.Infrastructure;

namespace HappyTravel.Edo.UnitTests.Infrastructure
{
    public class DateTimeProviderMock : IDateTimeProvider
    {
        public DateTimeProviderMock(DateTime dateTime)
        {
            _dateTime = dateTime;
        }


        private readonly DateTime _dateTime;

        public DateTime UtcNow() => _dateTime;
    }
}