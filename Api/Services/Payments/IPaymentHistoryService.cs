using System.Linq;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentHistoryService
    {
        Result<IQueryable<PaymentHistoryData>> GetAgentHistory(PaymentHistoryRequest paymentHistoryRequest, AgentContext agent);

        Result<IQueryable<PaymentHistoryData>> GetAgencyHistory(PaymentHistoryRequest paymentHistoryRequest, AgentContext agent );
    }
}