using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Models.Mailing;

public class ApiConnectionData : DataWithCompanyInfo
{
    public ApiConnectionData(Agency agency, Agent agent)
    {
        AgencyId = agency.Id.ToString();
        AgencyName = agency.Name;
        AgentId = agent.Id.ToString();
        AgentName = $"{agent.Title} {agent.FirstName} {agent.LastName}";
    }


    public string AgencyId { get; }
    public string AgencyName { get; }
    public string AgentId { get; }
    public string AgentName { get; }
}