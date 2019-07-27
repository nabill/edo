using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct RegisterMasterCustomerRequest
    {
        [JsonConstructor]
        public RegisterMasterCustomerRequest(CustomerRegistrationInfo masterCustomer, CompanyRegistrationInfo company)
        {
            MasterCustomer = masterCustomer;
            Company = company;
        }

        /// <summary>
        ///     Master customer personal information.
        /// </summary>
        public CustomerRegistrationInfo MasterCustomer { get; }

        /// <summary>
        ///     Customer affiliated company information.
        /// </summary>
        public CompanyRegistrationInfo Company { get; }
    }
}