using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct RegisterRegularCustomerRequest
    {
        [JsonConstructor]
        public RegisterRegularCustomerRequest(CustomerRegistrationInfo customerRegistrationInfo, string invitationCode)
        {
            CustomerRegistrationInfo = customerRegistrationInfo;
            InvitationCode = invitationCode;
        }
        
        public CustomerRegistrationInfo CustomerRegistrationInfo { get; }
        public string InvitationCode { get; }
    }
}