using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct RegisterInvitedCustomerRequest
    {
        [JsonConstructor]
        public RegisterInvitedCustomerRequest(CustomerEditableInfo registrationInfo, string invitationCode)
        {
            RegistrationInfo = registrationInfo;
            InvitationCode = invitationCode;
        }


        /// <summary>
        ///     Regular customer personal information.
        /// </summary>
        public CustomerEditableInfo RegistrationInfo { get; }

        /// <summary>
        ///     Invitation code.
        /// </summary>
        public string InvitationCode { get; }
    }
}