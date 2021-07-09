using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Infrastructure.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent<TEventData>(in TEventData eventData, string name, in AgentContext agent, in GeoPoint? point = default);
    }
}