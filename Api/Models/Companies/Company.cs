using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Companies
{
    public readonly struct Company
    {
        [JsonConstructor]
        public Company(string name, string address, string country, string city, string phone,
            string fax, string postalCode, PreferredCurrency preferredCurrency,
            PreferredPaymentMethod preferredPaymentMethod, string website)
        {
            Name = name;
            Address = address;
            Country = country;
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
        public string Country { get; }
        
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
        public PreferredCurrency PreferredCurrency { get; }
        
        [Required] 
        public PreferredPaymentMethod PreferredPaymentMethod { get; }
        
        [Url] 
        public string Website { get; }
    }
}