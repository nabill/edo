using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings;

public readonly struct IntermediaryBankDetails
{
    [JsonConstructor]
    public IntermediaryBankDetails(string bankName, string swiftCode, string accountNumber, string abaNo)
    {
        BankName = bankName;
        SwiftCode = swiftCode;
        AccountNumber = accountNumber;
        AbaNo = abaNo;
    }


    public string BankName { get; }
    public string SwiftCode { get; }
    public string AccountNumber { get; }
    public string AbaNo { get; }
}