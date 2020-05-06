using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public struct RegisterInvitedAgentRequest
    {
        [JsonConstructor]
        public RegisterInvitedAgentRequest(AgentEditableInfo registrationInfo, string invitationCode)
        {
            RegistrationInfo = registrationInfo;
            InvitationCode = invitationCode;
        }


        /// <summary>
        ///     Regular agent personal information.
        /// </summary>
        public AgentEditableInfo RegistrationInfo { get; }

        /// <summary>
        ///     Invitation code.
        /// </summary>
        public string InvitationCode { get; }
    }
}