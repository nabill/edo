using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings.Invoices
{
    public readonly struct SellerInfo
    {
        [JsonConstructor]
        public SellerInfo(string companyName, string bankName, string bankAddress, string accountNumber, string iban, string routingCode, string swiftCode,
            IntermediaryBankDetails? intermediaryBankDetails = null)
        {
            AccountNumber = accountNumber;
            BankAddress = bankAddress;
            BankName = bankName;
            CompanyName = companyName;
            Iban = iban;
            RoutingCode = routingCode;
            SwiftCode = swiftCode;
            IntermediaryBankDetails = intermediaryBankDetails;
        }


        public string AccountNumber { get; }
        public string BankAddress { get; }
        public string BankName { get; }
        public string CompanyName { get; }
        public string Iban { get; }
        public string RoutingCode { get; }
        public string SwiftCode { get; }
        public IntermediaryBankDetails? IntermediaryBankDetails { get; }
    }
}
