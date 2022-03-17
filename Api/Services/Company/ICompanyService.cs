using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data.Company;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Company
{
    public interface ICompanyService
    {
        Task<Result<CompanyInfo>> Get();
        Task<Result<CompanyAccount>> GetDefaultBankAccount(Currencies currency);
    }
}