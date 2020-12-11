namespace HappyTravel.Edo.Api.Infrastructure.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent(object eventData, string name);
    }
}