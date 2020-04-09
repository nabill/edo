using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentHistoryService
    {
        Task<Result<List<PaymentHistoryData>>> GetAgentHistory(PaymentHistoryRequest paymentHistoryRequest, int counterpartyId);

        Task<Result<List<PaymentHistoryData>>> GetCounterpartyHistory(PaymentHistoryRequest paymentHistoryRequest, int counterpartyId);
    }
}