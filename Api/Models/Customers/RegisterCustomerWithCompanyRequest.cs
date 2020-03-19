using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct RegisterCustomerWithCompanyRequest
    {
        [JsonConstructor]
        public RegisterCustomerWithCompanyRequest(CustomerEditableInfo customer, CompanyInfo company)
        {
            Customer = customer;
            Company = company;
        }


        /// <summary>
        ///     Customer personal information.
        /// </summary>
        public CustomerEditableInfo Customer { get; }

        /// <summary>
        ///     Customer affiliated company information.
        /// </summary>
        public CompanyInfo Company { get; }
    }
}