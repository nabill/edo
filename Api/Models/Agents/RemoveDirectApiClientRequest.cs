using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct RemoveDirectApiClientRequest
    {
        [JsonConstructor]
        public RemoveDirectApiClientRequest(int agentId, int agencyId, string clientId)
        {
            AgentId = agentId;
            AgencyId = agencyId;
            ClientId = clientId;
        }

        public int AgentId { get; }
        public int AgencyId { get; }
        public string ClientId { get; }
    }
}