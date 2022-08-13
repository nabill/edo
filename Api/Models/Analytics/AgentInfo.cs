namespace HappyTravel.Edo.Api.Models.Analytics;

public readonly struct AgentInfo
{
    public AgentInfo(int agentId, int agencyId, string agentName, string agencyName)
    {
        AgentId = agentId;
        AgencyId = agencyId;
        AgentName = agentName;
        AgencyName = agencyName;
    }

    /// <summary>
    /// Agent Id
    /// </summary>
    public int AgentId { get; }
    
    /// <summary>
    /// Agency Id
    /// </summary>
    public int AgencyId { get; }
    
    /// <summary>
    /// Agent name
    /// </summary>
    public string AgentName { get; }
    
    /// <summary>
    /// Agency name
    /// </summary>
    public string AgencyName { get; }
}