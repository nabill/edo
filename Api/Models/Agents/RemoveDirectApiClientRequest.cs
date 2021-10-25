using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct RemoveDirectApiClientRequest
    {
        [JsonConstructor]
        public RemoveDirectApiClientRequest(int agentId, string clientId)
        {
            AgentId = agentId;
            ClientId = clientId;
        }

        public int AgentId { get; }
        public string ClientId { get; }
    }
}