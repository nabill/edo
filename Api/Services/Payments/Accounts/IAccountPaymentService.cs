using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentService
    {
        Task<bool> CanPayWithAccount();
        Task<List<AgencyAccountInfo>> GetAgencyAccounts();
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency);
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, int agencyId);
        Task<Result<PaymentResponse>> Charge(string referenceCode, IPaymentCallbackService paymentCallbackService);
        Task<Result> Refund(string referenceCode, DateTimeOffset operationDate, IPaymentCallbackService paymentCallbackService, string reason);
        Task<Result> TransferToChildAgency(int payerAccountId, int recipientAccountId, MoneyAmount amount);
    }
}