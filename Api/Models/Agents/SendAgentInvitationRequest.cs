using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Users;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct SendAgentInvitationRequest
    {
        [JsonConstructor]
        public SendAgentInvitationRequest(UserDescriptionInfo registrationInfo, string email, int[] roleIds)
        {
            RegistrationInfo = registrationInfo;
            Email = email;
            RoleIds = roleIds;
        }


        /// <summary>
        ///     Regular agent personal information.
        /// </summary>
        [Required]
        public UserDescriptionInfo RegistrationInfo { get; }

        /// <summary>
        ///     E-mail for invitation.
        /// </summary>
        [Required]
        public string Email { get; }
        
        /// <summary>
        ///     Role ids assigned to user
        /// </summary>
        [Required]
        public int[] RoleIds { get; }
    }
}