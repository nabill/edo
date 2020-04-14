using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public struct RegisterAgentWithCounterpartyRequest
    {
        [JsonConstructor]
        public RegisterAgentWithCounterpartyRequest(AgentEditableInfo agent, CounterpartyInfo counterparty)
        {
            Agent = agent;
            Counterparty = counterparty;
        }


        /// <summary>
        ///     Agent personal information.
        /// </summary>
        public AgentEditableInfo Agent { get; }

        /// <summary>
        ///     Agent affiliated counterparty information.
        /// </summary>
        public CounterpartyInfo Counterparty { get; }
    }
}