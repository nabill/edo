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
        
        public CustomerRegistrationInfo RegistrationInfo { get; }
        public string InvitationCode { get; }
    }
}