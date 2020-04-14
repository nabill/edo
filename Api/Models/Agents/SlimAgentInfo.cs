using System;
using IdentityModel;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct SlimAgentInfo
    {
        [JsonConstructor]
        public SlimAgentInfo(int agentId, string firstName, string lastName, DateTime created,
            int counterpartyId, string counterpartyName, int agencyId, string agencyName, string markupSettings)
        {
            AgentId = agentId;
            Name = $"{firstName} {lastName}";
            Created = created.ToEpochTime();
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            AgencyId = agencyId;
            AgencyName = agencyName;
            MarkupSettings = markupSettings;
        }

        /// <summary>
        ///     Agent ID.
        /// </summary>
        public int AgentId { get; }

        /// <summary>
        ///     First and last name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Created date timestamp.
        /// </summary>
        public long Created { get; }

        /// <summary>
        ///     ID of the agent's counterparty.
        /// </summary>
        public int CounterpartyId { get; }

        /// <summary>
        ///     Name of the agent's counterparty.
        /// </summary>
        public string CounterpartyName { get; }

        /// <summary>
        ///     ID of the agent's agency.
        /// </summary>
        public int AgencyId { get; }

        /// <summary>
        ///     Name of the agent's agency.
        /// </summary>
        public string AgencyName { get; }

        /// <summary>
        ///     Markup settings of the agent.
        /// </summary>
        public string MarkupSettings { get; }
    }
}