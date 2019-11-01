using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.PaymentLinks;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinksProcessingService : IPaymentLinksProcessingService
    {
        public PaymentLinksProcessingService(IPayfortService payfortService,
            IPaymentLinkService linkService)
        {
            _payfortService = payfortService;
            _linkService = linkService;
        }


        public Task<Result<PaymentResponse>> Pay(string code, string token, string ip, string languageCode)
        {
            return GetLink(code)
                .OnSuccess(Pay)
                .Map(ToPaymentResponse);

            Task<Result<CreditCardPaymentResult>> Pay(PaymentLinkData link)
            {
                return _payfortService.Pay(new CreditCardPaymentRequest(
                    amount: link.Amount,
                    currency: link.Currency,
                    token: token,
                    customerName: string.Empty,
                    customerEmail: link.Email,
                    customerIp: ip,
                    referenceCode: link.ReferenceCode,
                    languageCode: languageCode));
            }

            PaymentResponse ToPaymentResponse(CreditCardPaymentResult cr) => new PaymentResponse(cr.Secure3d, cr.Status);
        }


        public Task<Result<PaymentResponse>> ProcessPaymentResponse(string code, JObject response)
        {
            return GetLinkToPay()
                .OnSuccess(ProcessCardResponse)
                .OnSuccess(StorePaymentResult);

            async Task<Result<PaymentLinkData>> GetLinkToPay()
            {
                var (_, isFailure, link, error) = await GetLink(code);
                if (isFailure)
                    return Result.Fail<PaymentLinkData>(error);

                return link.PaymentStatus == PaymentStatuses.Success
                    ? Result.Fail<PaymentLinkData>("Link is already paid")
                    : Result.Ok(link);
            }


            Result<PaymentResponse> ProcessCardResponse(PaymentLinkData link)
            {
                var (_, isFailure, cardPaymentResult, error) = _payfortService.ProcessPaymentResponse(response);
                if (isFailure)
                    return Result.Fail<PaymentResponse>(error);

                return Result.Ok(new PaymentResponse(cardPaymentResult.Message, cardPaymentResult.Status));
            }


            async Task<PaymentResponse> StorePaymentResult(PaymentResponse paymentResponse)
            {
                await _linkService.UpdatePaymentStatus(code, paymentResponse);
                return paymentResponse;
            }
        }
        private Task<Result<PaymentLinkData>> GetLink(string code) => _linkService.Get(code);
        
        private readonly IPayfortService _payfortService;
        private readonly IPaymentLinkService _linkService;
    }
}