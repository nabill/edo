using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Models.Mailing;

public class ApiConnectionData : DataWithCompanyInfo
{
    public ApiConnectionData(string agencyId, string agencyName, string agentId, string agentName)
    {
        AgencyId = agencyId;
        AgencyName = agencyName;
        AgentId = agentId;
        AgentName = agentName;
    }


    public string AgencyId { get; }
    public string AgencyName { get; }
    public string AgentId { get; }
    public string AgentName { get; }
}