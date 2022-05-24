using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Company;

public record CompanyAccountInfo
{
    public int Id { get; init; }
    public Currencies Currency { get; init; }
    public string AccountNumber { get; init; }
    public string Iban { get; init; }
    public CompanyBankInfo? CompanyBank { get; init; }
    public IntermediaryBank? IntermediaryBank { get; init; }
    
    public bool IsDefault { get; init; }
}
