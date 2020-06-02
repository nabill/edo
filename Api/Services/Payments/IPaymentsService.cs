using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentsService
    {
        Task<Result<MoneyAmount>> GetServicePrice(string referenceCode);

        Task<Result> ProcessPaymentChanges(Payment payment);
        
        Task<Result<AgentInfoInAgency>> GetServiceBuyer(string referenceCode);
    }
}