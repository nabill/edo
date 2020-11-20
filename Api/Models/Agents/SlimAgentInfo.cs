using System;
using IdentityModel;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct SlimAgentInfo
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
        ///     Markup settings of the agent.
        /// </summary>
        public string MarkupSettings { get; }

        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; }
    }
}