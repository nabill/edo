using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public interface IPaymentLinksProcessingService
    {
        Task<Result<PaymentResponse>> Pay(string code, string token, string ip, string languageCode);
        
        Task<Result<PaymentResponse>> Pay(string code, NGeniusPayByLinkRequest request, string ip, string languageCode);

        Task<Result<PaymentResponse>> ProcessResponse(string code, JObject value);
        
        Task<Result<PaymentResponse>> ProcessNGeniusWebhook(string code, CreditCardPaymentStatuses status);

        Task<Result<string>> CalculateSignature(string code, string merchantReference, string fingerprint, string languageCode);
    }
}