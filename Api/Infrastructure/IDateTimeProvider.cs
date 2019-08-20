using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow();
    }

    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}