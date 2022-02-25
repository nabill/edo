using System.Collections.Generic;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class BankDetails
    {
        public Dictionary<Currencies, CurrencySpecificData> AccountDetails { get; set; }
        public Dictionary<Currencies, IntermediaryBankData> IntermediaryBankDetails { get; set; }
        public string BankAddress { get; set; }
        public string CompanyName { get; set; }
        
        public string AccountNumber { get; set; }
        public string Iban { get; set; }
        public string BankName { get; set; }
        public string RoutingCode { get; set; }
        public string SwiftCode { get; set; }


        public class CurrencySpecificData
        {
            public string AccountNumber { get; set; }
            public string Iban { get; set; }
        }
        
        
        public class  IntermediaryBankData
        {
            public string BankName { get; set; }
            public string SwiftCode { get; set; }
            public string AccountNumber { get; set; }
            public string AbaNo { get; set; }
        }
    }
}