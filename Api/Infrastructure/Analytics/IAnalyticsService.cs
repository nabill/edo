using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Geography;

namespace HappyTravel.Edo.Api.Infrastructure.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent<TEventData>(in TEventData eventData, string name, in AgentContext agent, in GeoPoint? point = default);
    }
}