using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentsService
    {
        Task<Result<MoneyAmount>> GetServicePrice(string referenceCode);

        Task<Result> ProcessPaymentChanges(Payment payment);
        
        Task<Result<(int AgentId, int AgencyId)>> GetServiceBuyer(string referenceCode);
    }
}