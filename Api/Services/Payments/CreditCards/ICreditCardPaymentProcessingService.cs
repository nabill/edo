using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardPaymentProcessingService
    {
        Task<Result<PaymentResponse>> AuthorizeMoney(NewCreditCardPaymentRequest request, 
            string languageCode, string ipAddress, IServicePaymentsService servicePaymentsService);


        Task<Result<PaymentResponse>> AuthorizeMoney(SavedCreditCardPaymentRequest request, string languageCode, 
            string ipAddress, IServicePaymentsService servicePaymentsService);


        Task<Result<PaymentResponse>> ProcessPaymentResponse(JObject rawResponse, IServicePaymentsService servicePaymentsService);

        Task<Result<string>> CaptureMoney(string referenceCode, IServicePaymentsService servicePaymentsService);

        Task<Result> VoidMoney(string referenceCode, IServicePaymentsService servicePaymentsService);
    }
}