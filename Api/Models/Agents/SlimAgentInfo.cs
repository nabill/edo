using System;
using IdentityModel;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public struct SlimAgentInfo
    {
        [JsonConstructor]
        public SlimAgentInfo(int agentId, string firstName, string lastName, DateTime created, string markupSettings, bool isActive)
        {
            AgentId = agentId;
            Name = $"{firstName} {lastName}";
            Created = created.ToEpochTime();
            MarkupSettings = markupSettings;
            IsActive = isActive;
        }

        /// <summary>
        ///     Agent ID.
        /// </summary>
        public int AgentId { get; set; }

        /// <summary>
        ///     First and last name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Created date timestamp.
        /// </summary>
        public long Created { get; set; }

        /// <summary>
        ///     Markup settings of the agent.
        /// </summary>
        public string MarkupSettings { get; set; }

        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; set; }
    }
}