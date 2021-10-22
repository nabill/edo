using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CreateDirectApiClientRequest
    {
        [JsonConstructor]
        public CreateDirectApiClientRequest(int agentId, string clientId, string name, string password)
        {
            AgentId = agentId;
            ClientId = clientId;
            Name = name;
            Password = password;
        }

        public int AgentId { get; }
        public string ClientId { get; }
        public string Name { get; }
        public string Password { get; }
    }
}