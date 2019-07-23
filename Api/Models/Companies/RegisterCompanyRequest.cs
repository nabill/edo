using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Companies
{
    public struct RegisterCompanyRequest
    {
        [JsonConstructor]
        public RegisterCompanyRequest(Company company, Customer masterCustomer)
        {
            Company = company;
            MasterCustomer = masterCustomer;
        }
        
        [Required]
        public Company Company { get; }
        
        [Required]
        public Customer MasterCustomer { get; }
    }
}