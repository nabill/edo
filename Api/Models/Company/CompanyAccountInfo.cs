using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Company;

public record CompanyAccountInfo
{
    public Currencies Currency { get; init; }
    public string AccountNumber { get; init; }
    public string Iban { get; init; }
    public string BankAddress { get; init; }
    public string BankName { get; init; }
    public string RoutingCode { get; init; }
    public string SwiftCode { get; init; }
    public IntermediaryBank IntermediaryBank { get; init; }
}
