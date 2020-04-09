using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentInvitationInfo
    {
        [JsonConstructor]
        public AgentInvitationInfo(AgentEditableInfo registrationInfo, int counterpartyId, string email)
        {
            RegistrationInfo = registrationInfo;
            CounterpartyId = counterpartyId;
            Email = email;
        }


        /// <summary>
        ///     Regular agent personal information.
        /// </summary>
        [Required]
        public AgentEditableInfo RegistrationInfo { get; }

        /// <summary>
        ///     Related counterparty id.
        /// </summary>
        [Required]
        public int CounterpartyId { get; }

        /// <summary>
        ///     E-mail for invitation.
        /// </summary>
        [Required]
        public string Email { get; }
    }
}