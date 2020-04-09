using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct AccountPaymentInfo
    {
        [JsonConstructor]
        public AccountPaymentInfo(string agentIp)
        {
            AgentIp = agentIp;
        }


        public string AgentIp { get; }
    }
}