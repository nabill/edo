using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentInvitationInfo
    {
        [JsonConstructor]
        public AgentInvitationInfo(AgentEditableInfo registrationInfo, int agencyId, int agentId, string email)
        {
            RegistrationInfo = registrationInfo;
            AgencyId = agencyId;
            Email = email;
            AgentId = agentId;
        }


        /// <summary>
        ///     Regular agent personal information.
        /// </summary>
        [Required]
        public AgentEditableInfo RegistrationInfo { get; }

        /// <summary>
        ///     Related agency id.
        /// </summary>
        [Required]
        public int AgencyId { get; }

        /// <summary>
        ///    Inviter agent id
        /// </summary>
        [Required]
        public int AgentId { get; }

        /// <summary>
        ///     E-mail for invitation.
        /// </summary>
        [Required]
        public string Email { get; }
    }
}