using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class RegistrationDataForAdmin : DataWithCompanyInfo
    {
        public CounterpartyRegistrationMailData Counterparty { get; set; }
        public string AgentEmail { get; set; }
        public string AgentName { get; set; }


        public struct CounterpartyRegistrationMailData
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public PaymentTypes PreferredPaymentMethod { get; set; }
            public string Website { get; set; }
            public string CountryCode { get; set; }
            public string City { get; set; }
            public string Phone { get; set; }
            public string Fax { get; set; }
            public string PostalCode { get; set; }
            public Currencies PreferredCurrency { get; set; }
        }
    }
}