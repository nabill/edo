using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Customers
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string PostalCode { get; set; }
        public Currency PreferredCurrency { get; set; }
        public PaymentMethod PreferredPaymentMethod { get; set; }
        public string Website { get; set; }
        public CompanyState State { get; set; }
    }
}