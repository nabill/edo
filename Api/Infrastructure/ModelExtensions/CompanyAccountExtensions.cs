using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data.Company;

namespace HappyTravel.Edo.Api.Infrastructure.ModelExtensions;

public static class CompanyAccountExtensions
{
    public static CompanyAccountInfo ToCompanyAccountInfo(this CompanyAccount companyAccount)
    {
        return new CompanyAccountInfo
        {
            Id = companyAccount.Id,
            Currency = companyAccount.Currency,
            AccountNumber = companyAccount.AccountNumber,
            Iban = companyAccount.Iban,
            CompanyBank = companyAccount.CompanyBank?.ToCompanyBankInfo(),
            IsDefault = companyAccount.IsDefault,
            IntermediaryBank = new IntermediaryBank
            {
                BankName = companyAccount.IntermediaryBankName,
                AccountNumber = companyAccount.IntermediaryBankAccountNumber,
                SwiftCode = companyAccount.IntermediaryBankSwiftCode,
                AbaNo = companyAccount.IntermediaryBankAbaNo
            }
        };
    }
    
    public static CompanyAccount ToCompanyAccount(this CompanyAccountInfo companyAccountInfo)
    {
        return new CompanyAccount
        {
            Currency = companyAccountInfo.Currency,
            AccountNumber = companyAccountInfo.AccountNumber,
            Iban = companyAccountInfo.Iban,
            CompanyBankId = companyAccountInfo.CompanyBank.Id,
            IntermediaryBankName = companyAccountInfo.IntermediaryBank.BankName,
            IntermediaryBankAccountNumber = companyAccountInfo.IntermediaryBank.AccountNumber,
            IntermediaryBankSwiftCode = companyAccountInfo.IntermediaryBank.SwiftCode,
            IntermediaryBankAbaNo = companyAccountInfo.IntermediaryBank.AbaNo,
            IsDefault = companyAccountInfo.IsDefault
        };
    }
    
    public static CompanyBankInfo ToCompanyBankInfo(this CompanyBank companyBank)
    {
        return new CompanyBankInfo
        {
            Id = companyBank.Id,
            Name = companyBank.Name,
            RoutingCode = companyBank.RoutingCode,
            SwiftCode = companyBank.SwiftCode,
            Address = companyBank.Address,
        };
    }
    
    public static CompanyBank ToCompanyBank(this CompanyBankInfo companyBank)
    {
        return new CompanyBank
        {
            Name = companyBank.Name,
            RoutingCode = companyBank.RoutingCode,
            SwiftCode = companyBank.SwiftCode,
            Address = companyBank.Address,
        };
    }
}
