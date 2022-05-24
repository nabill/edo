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
        public CompanyAccountService(EdoContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
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
                var bank = companyBankInfo.ToCompanyBank();
                bank.Created = _dateTimeProvider.UtcNow();
                bank.Modified = _dateTimeProvider.UtcNow();
                _context.CompanyBanks.Add(bank);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> ModifyBank(int bankId, CompanyBankInfo bankInfo)
        {
            return await ValidateBankInfo(bankInfo)
                .Ensure(IsUnique, "A bank with the same name already exists")
                .Bind(() => GetBank(bankId))
                .Tap(Modify);


            async Task<bool> IsUnique()
                => !await _context.CompanyBanks
                    .AnyAsync(bank => (bank.Name.ToLower() == bankInfo.Name.ToLower()
                        && bank.Id != bankId));


            Task Modify(CompanyBank bank)
            {
                bank.Name = bankInfo.Name;
                bank.Address = bankInfo.Address;
                bank.RoutingCode = bankInfo.RoutingCode;
                bank.SwiftCode = bankInfo.SwiftCode;
                bank.Modified = _dateTimeProvider.UtcNow();
                _context.CompanyBanks.Update(bank);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> RemoveBank(int bankId)
        {
            return await GetBank(bankId)
                .Ensure(IsUnused, "This bank is in use and cannot be removed")
                .Tap(Remove);


            async Task<bool> IsUnused(CompanyBank _)
                => !await _context.CompanyAccounts.AnyAsync(account => account.IsDefault && account.CompanyBankId == bankId);


            Task Remove(CompanyBank companyBank)
            {
                _context.CompanyBanks.Remove(companyBank);
                return _context.SaveChangesAsync();
            }
        }


        public Task<Result<List<CompanyAccountInfo>>> GetAccounts(int bankId)
        {
            return IsBankExists(bankId)
                .Bind(() => GetBankAccounts(bankId));
        }


        public Task<Result> AddAccount(int bankId, CompanyAccountInfo accountInfo)
        {
            return IsBankExists(bankId)
                .Bind(() => ValidateAccountInfo(accountInfo))
                .Tap(Add);


            Task Add()
            {
                var newAccount = accountInfo.ToCompanyAccount();
                newAccount.Created = _dateTimeProvider.UtcNow();
                newAccount.Modified = _dateTimeProvider.UtcNow();
                newAccount.IsDefault = false;
                _context.CompanyAccounts.Add(newAccount);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> ModifyAccount(int bankId, int accountId, CompanyAccountInfo accountInfo)
        {
            return await ValidateAccountInfo(accountInfo)
                .Bind(() => GetAccount(bankId, accountId))
                .Tap(Modify);


            Task Modify(CompanyAccount account)
            {
                account.Currency = accountInfo.Currency;
                account.Iban = accountInfo.Iban;
                account.AccountNumber = accountInfo.AccountNumber;
                account.IntermediaryBankName = accountInfo.IntermediaryBank?.BankName;
                account.IntermediaryBankAccountNumber = accountInfo.IntermediaryBank?.AccountNumber;
                account.IntermediaryBankSwiftCode = accountInfo.IntermediaryBank?.SwiftCode;
                account.IntermediaryBankAbaNo = accountInfo.IntermediaryBank?.AbaNo;
                account.Modified = _dateTimeProvider.UtcNow();

                _context.CompanyAccounts.Update(account);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> RemoveAccount(int bankId, int accountId)
        {
            return await GetAccount(bankId, accountId)
                .Ensure(IsNotDefault, "This account is selected as default and cannot be removed")
                .Tap(Remove);

            bool IsNotDefault(CompanyAccount account) => !account.IsDefault;


            Task Remove(CompanyAccount companyAccount)
            {
                _context.CompanyAccounts.Remove(companyAccount);
                return _context.SaveChangesAsync();
            }
        }


        private async Task<Result<CompanyBank>> GetBank(int bankId)
            => await _context.CompanyBanks
                    .SingleOrDefaultAsync(r => r.Id == bankId)
                ?? Result.Failure<CompanyBank>($"Bank with Id {bankId} does not exist");
        
        private async Task<Result> IsBankExists(int bankId)
            => await _context.CompanyBanks
                .AnyAsync(r => r.Id == bankId) 
                ? Result.Success()
                : Result.Failure<CompanyBank>($"Bank with Id {bankId} does not exist");


        private async Task<Result<CompanyAccount>> GetAccount(int bankId, int accountId)
            => await _context.CompanyAccounts
                    .SingleOrDefaultAsync(r => r.Id == accountId && r.CompanyBankId == bankId)
                ?? Result.Failure<CompanyAccount>($"An account Id {accountId} and CompanyBankId {bankId} does not exist");


        private async Task<Result<List<CompanyAccountInfo>>> GetBankAccounts(int bankId)
            => await _context.CompanyAccounts
                .Where(r => r.CompanyBankId == bankId)
                .Select(r => r.ToCompanyAccountInfo())
                .ToListAsync();


        private static Result ValidateBankInfo(CompanyBankInfo companyBankInfo)
            => GenericValidator<CompanyBankInfo>.Validate(v =>
                {
                    v.RuleFor(r => r.Name).NotEmpty();
                    v.RuleFor(r => r.Address).NotEmpty();
                    v.RuleFor(r => r.RoutingCode).NotEmpty();
                    v.RuleFor(r => r.SwiftCode).NotEmpty();
                },
                companyBankInfo);


        private static Result ValidateAccountInfo(CompanyAccountInfo companyBankInfo)
            => GenericValidator<CompanyAccountInfo>.Validate(v =>
                {
                    v.RuleFor(r => r.Currency).NotEmpty();
                    v.RuleFor(r => r.AccountNumber).NotEmpty();
                    v.RuleFor(r => r.Iban).NotEmpty();
                    
                    v.RuleFor(r => r.IntermediaryBank!.AccountNumber).NotEmpty().When(r => r.IntermediaryBank is not null);
                    v.RuleFor(r => r.IntermediaryBank!.BankName).NotEmpty().When(r => r.IntermediaryBank is not null);
                    v.RuleFor(r => r.IntermediaryBank!.SwiftCode).NotEmpty().When(r => r.IntermediaryBank is not null);
                    v.RuleFor(r => r.IntermediaryBank!.AbaNo).NotEmpty().When(r => r.IntermediaryBank is not null);
                },
                companyBankInfo);


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}