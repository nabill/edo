using HappyTravel.Edo.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.ModelExtensions;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data.Company;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CompanyAccountService : ICompanyAccountService
    {
        public CompanyAccountService(EdoContext context)
        {
            _context = context;
        }

        public Task<List<CompanyBankInfo>> GetAllBanks()
        {
            return _context.CompanyBanks.Select(companyBank => companyBank.ToCompanyBankInfo()).ToListAsync();
        }


        public Task<Result> AddBank(CompanyBankInfo companyBankInfo)
        {
            return ValidateBankInfo(companyBankInfo)
                .Ensure(IsUnique, "A bank with the same name already exists")
                .Tap(Add);


            async Task<bool> IsUnique()
                => !await _context.CompanyBanks
                    .AnyAsync(bank => bank.Name.ToLower() == companyBankInfo.Name.ToLower());

            Task Add()
            {
                _context.CompanyBanks.Add(companyBankInfo.ToCompanyBank());
                return _context.SaveChangesAsync();
            }
        }

        public async Task<Result> EditBank(int bankId, CompanyBankInfo bankInfo)
        {
            return await ValidateBankInfo(bankInfo)
                .Ensure(IsUnique, "A bank with the same name already exists")
                .Bind(() => GetBank(bankId))
                .Tap(Edit);


            async Task<bool> IsUnique()
                => !await _context.CompanyBanks
                    .AnyAsync(bank => (bank.Name.ToLower() == bankInfo.Name.ToLower() 
                        && bank.Id != bankInfo.Id));


            Task Edit(CompanyBank bank)
            {
                bank.Name = bankInfo.Name;
                bank.Address = bankInfo.Address;
                bank.RoutingCode = bankInfo.RoutingCode;
                bank.SwiftCode = bankInfo.SwiftCode;
                
                _context.Update(bank);
                return _context.SaveChangesAsync();
            }
        }
        
        public async Task<Result> DeleteBank(int bankId)
        {
            return await GetBank(bankId)
                .Ensure(IsUnused, "This bank is in use and cannot be deleted")
                .Tap(Delete);


             async Task<bool> IsUnused(CompanyBank _)
                 => !await _context.CompanyAccounts.AnyAsync(account => account.IsDefault && account.CompanyBankId == bankId);


            Task Delete(CompanyBank companyBank)
            {
                _context.CompanyBanks.Remove(companyBank);
                return _context.SaveChangesAsync();
            }
        }
        
        private async Task<Result<CompanyBank>> GetBank(int bankId)
            => await _context.CompanyBanks
                    .SingleOrDefaultAsync(r => r.Id == bankId)
                ?? Result.Failure<CompanyBank>("A bank with specified Id does not exist");

        private static Result ValidateBankInfo(CompanyBankInfo companyBankInfo)
            => GenericValidator<CompanyBankInfo>.Validate(v =>
                {
                    v.RuleFor(r => r.Name).NotEmpty();
                },
                companyBankInfo);
        
        private readonly EdoContext _context;
    }
}