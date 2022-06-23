using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Money.Enums;

namespace Api.AdministratorServices
{
    public interface ICompanyInfoService
    {
        Task Update(CompanyInfo companyInfo, CancellationToken cancellationToken = default);
        Task<Result<CompanyInfo>> Get(CancellationToken cancellationToken = default);
        Task<Result<CompanyAccountInfo>> GetDefaultBankAccount(Currencies currency, CancellationToken cancellationToken = default);
    }
}