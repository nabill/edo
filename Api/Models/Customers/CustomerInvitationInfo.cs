using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInvitationInfo
    {
        [JsonConstructor]
        public CustomerInvitationInfo(CustomerRegistrationInfo registrationInfo, int companyId)
        {
            RegistrationInfo = registrationInfo;
            CompanyId = companyId;
        }
        
        /// <summary>
        /// Regular customer personal information.
        /// </summary>
        public CustomerRegistrationInfo RegistrationInfo { get; }
        
        /// <summary>
        /// Related company id.
        /// </summary>
        public int CompanyId { get; }
    }
}