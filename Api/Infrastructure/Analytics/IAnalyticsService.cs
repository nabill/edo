using HappyTravel.Edo.Api.Models.Analytics;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Infrastructure.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent<TEventData>(in TEventData eventData, string name, in AgentAnalyticsInfo agentAnalyticsInfo, in GeoPoint? point = default);
    }
}