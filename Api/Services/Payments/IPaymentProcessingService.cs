using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentProcessingService
    {
        Task<Result> AddMoney(int accountId, decimal amount);
        Task<Result> ChargeMoney(int accountId, decimal amount);
    }
}