using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Companies
{
    public readonly struct CompanyRegistrationInfo
    {
        [JsonConstructor]
        public CompanyRegistrationInfo(string name, string address, string countryCode, string city, string phone,
            string fax, string postalCode, Currency preferredCurrency,
            PaymentMethod preferredPaymentMethod, string website)
        {
            Name = name;
            Address = address;
            CountryCode = countryCode;
            City = city;
            Phone = phone;
            Fax = fax;
            PostalCode = postalCode;
            PreferredCurrency = preferredCurrency;
            PreferredPaymentMethod = preferredPaymentMethod;
            Website = website;
        }

        [Required] 
        public string Name { get; }
        
        [Required] 
        public string Address { get; }
        
        [Required] 
        public string CountryCode { get; }
        
        [Required] 
        public string City { get; }

        [Required]
        [Phone]
        [RegularExpression(@"^[0-9]{3,30}$")]
        public string Phone { get; }

        [Phone]
        [RegularExpression(@"^[0-9]{3,30}$")]
        public string Fax { get; }

        [DataType(DataType.PostalCode)]
        public string PostalCode { get; }
        
        [Required] 
        public Currency PreferredCurrency { get; }
        
        [Required] 
        public PaymentMethod PreferredPaymentMethod { get; }
        
        [Url] 
        public string Website { get; }
    }
}