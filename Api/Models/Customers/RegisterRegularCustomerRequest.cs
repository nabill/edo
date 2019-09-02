using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct RegisterRegularCustomerRequest
    {
        [JsonConstructor]
        public RegisterRegularCustomerRequest(CustomerRegistrationInfo registrationInfo, string invitationCode)
        {
            RegistrationInfo = registrationInfo;
            InvitationCode = invitationCode;
        }
        
        /// <summary>
        /// Regular customer personal information.
        /// </summary>
        public CustomerRegistrationInfo RegistrationInfo { get; }
        
        /// <summary>
        /// Invitation code.
        /// </summary>
        public string InvitationCode { get; }
    }
}