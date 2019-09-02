using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct RegularCustomerInvitation
    {
        [JsonConstructor]
        public RegularCustomerInvitation(CustomerRegistrationInfo customerRegistrationInfo, int companyId)
        {
            CustomerRegistrationInfo = customerRegistrationInfo;
            CompanyId = companyId;
        }
        
        public CustomerRegistrationInfo CustomerRegistrationInfo { get; }
        public int CompanyId { get; }
    }
}