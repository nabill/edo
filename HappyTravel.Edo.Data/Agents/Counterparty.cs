using System;

namespace HappyTravel.Edo.Data.Agents
{
    public class Counterparty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsContractUploaded { get; set; }
        public string Address { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string PostalCode { get; set; }
        public string Website { get; set; }
        public string VatNumber { get; set; }
        public string BillingEmail { get; set; }
    }
}