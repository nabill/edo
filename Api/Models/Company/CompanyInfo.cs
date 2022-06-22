using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Company
{
    public class CompanyInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PostalCode { get; set; }
        public string Trn { get; set; }
        public string TradeLicense { get; set; }
        public List<string> AvailableCurrencies { get; set; }
    }
}