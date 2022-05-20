using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data.Company;

namespace HappyTravel.Edo.Api.Infrastructure.ModelExtensions;

public static class CompanyAccountExtensions
{
    public static CompanyAccountInfo ToCompanyAccount(this CompanyAccount companyAccount)
    {
        return new CompanyAccountInfo
        {
            Currency = companyAccount.Currency,
            AccountNumber = companyAccount.AccountNumber,
            Iban = companyAccount.Iban,
            BankAddress = companyAccount.CompanyBank.Address,
            BankName = companyAccount.CompanyBank.Name,
            RoutingCode = companyAccount.CompanyBank.RoutingCode,
            SwiftCode = companyAccount.CompanyBank.SwiftCode,
            IntermediaryBank = new IntermediaryBank
            {
                BankName = companyAccount.IntermediaryBankName,
                AccountNumber = companyAccount.IntermediaryBankAccountNumber,
                SwiftCode = companyAccount.IntermediaryBankSwiftCode,
                AbaNo = companyAccount.IntermediaryBankAbaNo
            }
        };
    }
    
    public static CompanyBankInfo ToCompanyBankInfo(this CompanyBank companyBank)
    {
        return new CompanyBankInfo
        {
            Name = companyBank.Name,
            RoutingCode = companyBank.RoutingCode,
            SwiftCode = companyBank.SwiftCode,
            Address = companyBank.Address,
            Created = companyBank.Created,
            Modified = companyBank.Modified,
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
            Created = companyBank.Created,
            Modified = companyBank.Modified,
        };
    }
}
