using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CreateDirectApiClientRequest
    {
        [JsonConstructor]
        public CreateDirectApiClientRequest(int agentId, int agencyId, string clientId, string name, string password)
        {
            AgentId = agentId;
            AgencyId = agencyId;
            ClientId = clientId;
            Name = name;
            Password = password;
        }

        public int AgentId { get; }
        public int AgencyId { get; }
        public string ClientId { get; }
        public string Name { get; }
        public string Password { get; }
    }
}