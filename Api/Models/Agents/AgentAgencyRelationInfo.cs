using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentAgencyRelationInfo
    {
        public AgentAgencyRelationInfo(int id, string name, int agencyId, string agencyName, bool isMaster, List<InAgencyPermissions> inAgencyPermissions,
            AgencyVerificationStates agencyVerificationState, PaymentTypes defaultPaymentType)
        {
            Id = id;
            Name = name;
            AgencyId = agencyId;
            AgencyName = agencyName;
            IsMaster = isMaster;
            InAgencyPermissions = inAgencyPermissions;
            AgencyVerificationState = agencyVerificationState;
            DefaultPaymentType = defaultPaymentType;
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
        public List<InAgencyPermissions> InAgencyPermissions { get; }

        /// <summary>
        /// State of the counterparty
        /// </summary>
        public AgencyVerificationStates AgencyVerificationState { get; }

        /// <summary>
        /// Default payment type
        /// </summary>
        public PaymentTypes DefaultPaymentType { get; }
    }
}