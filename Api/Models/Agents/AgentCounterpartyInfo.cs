using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentCounterpartyInfo
    {
        public AgentCounterpartyInfo(int id, string name, int agencyId, string agencyName, bool isMaster, List<InCounterpartyPermissions> inCounterpartyPermissions)
        {
            Id = id;
            Name = name;
            AgencyId = agencyId;
            AgencyName = agencyName;
            IsMaster = isMaster;
            InCounterpartyPermissions = inCounterpartyPermissions;
        }


        /// <summary>
        ///     Id of the counterparty.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Name of the counterparty.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Id of the agency of the counterparty, to which the agent belongs.
        /// </summary>
        public int AgencyId { get; }

        /// <summary>
        ///     Name of the agency.
        /// </summary>
        public string AgencyName { get; }

        /// <summary>
        ///     Flag indicating that agent is master in this counterparty.
        /// </summary>
        public bool IsMaster { get; }

        /// <summary>
        ///     List of permissions in current counterparty.
        /// </summary>
        public List<InCounterpartyPermissions> InCounterpartyPermissions { get; }
    }
}