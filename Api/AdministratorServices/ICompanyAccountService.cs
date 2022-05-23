using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Company;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICompanyAccountService
    {
        Task<List<CompanyBankInfo>> GetAllBanks();

        Task<Result> AddBank(CompanyBankInfo bank);
        
        Task<Result> ModifyBank(int bankId, CompanyBankInfo bank);
        
        Task<Result> RemoveBank(int id);
    }
}