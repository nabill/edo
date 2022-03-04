using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentCallbackService
    {
        Task<Result<MoneyAmount>> GetChargingAmount(string referenceCode);

        Task<Result<MoneyAmount>> GetRefundableAmount(string referenceCode, DateTimeOffset operationDate);

        Task<Result> ProcessPaymentChanges(Payment payment);
        
        Task<Result<(int AgentId, int AgencyId)>> GetServiceBuyer(string referenceCode);
        
        Task<Result<int>> GetChargingAccountId(string referenceCode);
    }
}