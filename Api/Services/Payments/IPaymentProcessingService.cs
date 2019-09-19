using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Users;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentProcessingService
    {
        Task<Result> AddMoney(int accountId, PaymentData paymentData, UserInfo user);
        Task<Result> ChargeMoney(int accountId, PaymentData paymentData, UserInfo user);
    }
}