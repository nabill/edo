using HappyTravel.Money.Enums;
using System;

namespace HappyTravel.Edo.Data.Company;

public class CompanyAccount
{
    public int Id { get; set; }
    public int CompanyBankId { get; set; }
    public Currencies Currency { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? IntermediaryBankName { get; set; }
    public string? IntermediaryBankAccountNumber { get; set; }
    public string? IntermediaryBankSwiftCode { get; set; }
    public string? IntermediaryBankAbaNo { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }

    public CompanyBank CompanyBank { get; set; } = null!;
}
