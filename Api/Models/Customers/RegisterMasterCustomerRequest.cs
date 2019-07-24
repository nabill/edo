using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Companies
{
    public struct RegisterMasterCustomerRequest
    {
        public CustomerRegistrationInfo MasterCustomer { get; }
        public CompanyRegistrationInfo Company { get; }

        [JsonConstructor]
        public RegisterMasterCustomerRequest(CustomerRegistrationInfo masterCustomer, CompanyRegistrationInfo company)
        {
            MasterCustomer = masterCustomer;
            Company = company;
        }
    }
}