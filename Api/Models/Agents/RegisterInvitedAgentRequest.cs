using HappyTravel.Edo.Api.Models.Users;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct RegisterInvitedAgentRequest
    {
        [JsonConstructor]
        public RegisterInvitedAgentRequest(UserDescriptionInfo registrationInfo, string invitationCode)
        {
            RegistrationInfo = registrationInfo;
            InvitationCode = invitationCode;
        }


        /// <summary>
        ///     Regular agent personal information.
        /// </summary>
        public UserDescriptionInfo RegistrationInfo { get; }

        /// <summary>
        ///     Invitation code.
        /// </summary>
        public string InvitationCode { get; }
    }
}