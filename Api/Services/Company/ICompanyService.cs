using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Company
{
    public interface ICompanyService
    {
        Task<Result<CompanyInfo>> GetCompanyInfo();
        Task<Result<CompanyAccount>> GetDefaultBankAccount(Currencies currency);
    }
}