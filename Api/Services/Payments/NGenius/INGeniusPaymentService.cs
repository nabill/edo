using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public interface INGeniusPaymentService
    {
        Task<Result<NGeniusPaymentResponse>> Authorize(NewCreditCardRequest request, string ipAddress, AgentContext agent);

        Task<Result<NGeniusPaymentResponse>> Authorize(SavedCreditCardRequest request, string ipAddress, AgentContext agent);

        Task<Result<NGeniusPaymentResponse>> Pay(NewCreditCardRequest request, string ipAddress, string email, NGeniusBillingAddress billingAddress);

        Task<Result<CreditCardCaptureResult>> Capture(string paymentId, string orderReference, MoneyAmount amount);

        Task<Result<CreditCardVoidResult>> Void(string paymentId, string orderReference);
        
        Task<Result<CreditCardRefundResult>> Refund(string paymentId, string orderReference, string captureId, MoneyAmount amount);

        Task<Result<CreditCardPaymentStatuses>> NGenius3DSecureCallback(string referenceCode, NGenius3DSecureData data);
    }
}