using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public interface INGeniusPaymentService
    {
        Task<Result<NGeniusPaymentResponse>> Authorize(NewCreditCardRequest request, string ipAddress, AgentContext agent);

        Task<Result<NGeniusPaymentResponse>> Authorize(SavedCreditCardRequest request, string ipAddress, AgentContext agent);

        Task<Result<NGeniusPaymentResponse>> Pay(NewCreditCardRequest request, string ipAddress, AgentContext agent);

        Task<Result<CreditCardPaymentStatuses>> NGenius3DSecureCallback(string paymentId, string orderReference, NGenius3DSecureData data);
    }
}