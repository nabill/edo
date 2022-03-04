using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
        
        DateTimeOffset UtcTomorrow();
        
        DateTimeOffset UtcToday();
    }
}