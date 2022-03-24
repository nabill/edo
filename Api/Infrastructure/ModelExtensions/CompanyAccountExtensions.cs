using ApiModels = HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data.Company;

namespace HappyTravel.Edo.Api.Infrastructure.ModelExtensions;

public static class CompanyAccountExtensions
{
    public static ApiModels.CompanyAccount ToCompanyAccount(this CompanyAccount companyAccount)
    {
        return new ApiModels.CompanyAccount
        {
            Currency = companyAccount.Currency,
            AccountNumber = companyAccount.AccountNumber,
            Iban = companyAccount.Iban,
            BankAddress = companyAccount.CompanyBank.Address,
            BankName = companyAccount.CompanyBank.Name,
            RoutingCode = companyAccount.CompanyBank.RoutingCode,
            SwiftCode = companyAccount.CompanyBank.SwiftCode,
            IntermediaryBank = new ApiModels.IntermediaryBank
            {
                BankName = companyAccount.IntermediaryBankName,
                AccountNumber = companyAccount.IntermediaryBankAccountNumber,
                SwiftCode = companyAccount.IntermediaryBankSwiftCode,
                AbaNo = companyAccount.IntermediaryBankAbaNo
            }
        };
    }
}
