using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Invitations
{
    public readonly struct UserInvitationData
    {
        [JsonConstructor]
        public UserInvitationData(AgentEditableInfo agentRegistrationInfo,
            AgencyInfo childAgencyRegistrationInfo)
        {
            AgentRegistrationInfo = agentRegistrationInfo;
            ChildAgencyRegistrationInfo = childAgencyRegistrationInfo;
        }

        /// <summary>
        /// Prefilled user registration info.
        /// </summary>
        [Required]
        public AgentEditableInfo AgentRegistrationInfo { get; }

        /// <summary>
        /// Prefilled child agency registration info. Used only for child agency invitations.
        /// </summary>
        public AgencyInfo ChildAgencyRegistrationInfo { get; }
    }
}
