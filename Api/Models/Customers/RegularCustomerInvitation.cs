using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct RegularCustomerInvitation
    {
        [JsonConstructor]
        public RegularCustomerInvitation(CustomerRegistrationInfo registrationInfo, int companyId)
        {
            RegistrationInfo = registrationInfo;
            CompanyId = companyId;
        }
        
        public CustomerRegistrationInfo RegistrationInfo { get; }
        public int CompanyId { get; }
    }
}