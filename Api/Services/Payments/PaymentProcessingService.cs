using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentProcessingService : IPaymentProcessingService
    {
        private readonly EdoContext _context;
        private readonly IEntityLocker _locker;

        public PaymentProcessingService(EdoContext context, IEntityLocker locker)
        {
            _context = context;
            _locker = locker;
        }
        
        public Task<Result> AddMoney(int accountId, decimal amount)
        {
            return GetAccount(accountId)
                .OnSuccess(LockAccount)
                .OnSuccessWithTransaction(_context, account => Result.Ok(account)
                    .OnSuccess(AddMoney)
                    .OnSuccess(WriteAuditLog)
                )
                .OnBoth(UnlockAccount);
            
            async Task<PaymentAccount> AddMoney(PaymentAccount account)
            {
                account.Balance += amount;
                await _context.SaveChangesAsync();
                return account;
            }

            async Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
            {
                // TODO add payment log
                return account;
            }
            
            async Task<Result> UnlockAccount(Result<PaymentAccount> result)
            {
                await _locker.Release<PaymentAccount>(accountId);
                return result;
            }
        }

        public Task<Result> ChargeMoney(int accountId, decimal amount)
        {
            return GetAccount(accountId)
                .OnSuccess(LockAccount)
                .Ensure(BalanceIsSufficient, "Could not charge money, insufficient balance")
                .OnSuccessWithTransaction(_context, account => Result.Ok(account)
                    .OnSuccess(ChargeMoney)
                    .OnSuccess(WriteAuditLog)
                )
                .OnBoth(UnlockAccount);
            
            async Task<PaymentAccount> ChargeMoney(PaymentAccount account)
            {
                account.Balance -= amount;
                await _context.SaveChangesAsync();
                return account;
            }
            
            async Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
            {
                // TODO add payment log
                return account;
            }

            bool BalanceIsSufficient(PaymentAccount account)
            {
                return account.Balance + account.CreditLimit >= amount;
            }
            
            async Task<Result> UnlockAccount(Result<PaymentAccount> result)
            {
                await _locker.Release<PaymentAccount>(accountId);
                return result;
            }
        }
        
        private async Task<Result<PaymentAccount>> GetAccount(int accountId)
        {
            var account = await _context.PaymentAccounts.SingleOrDefaultAsync(p => p.Id == accountId);
            return account == default
                ? Result.Fail<PaymentAccount>("Could not find account")
                : Result.Ok(account);
        }
        
        private async Task<Result<PaymentAccount>> LockAccount(PaymentAccount account)
        {
            var (isSuccess, _, error) = await _locker.Acquire<PaymentAccount>(account.Id, nameof(IPaymentProcessingService));
            return isSuccess
                ? Result.Ok(account)
                : Result.Fail<PaymentAccount>(error);
        }
    }
}