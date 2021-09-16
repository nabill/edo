using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments.External
{
    public class PaymentCallbackDispatcher : IPaymentCallbackDispatcher
    {
        public PaymentCallbackDispatcher(ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            IPayfortResponseParser responseParser,
            IBookingPaymentCallbackService bookingPaymentCallbackService,
            IPaymentLinksProcessingService linksProcessingService,
            ITagProcessor tagProcessor,
            EdoContext context)
        {
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _responseParser = responseParser;
            _bookingPaymentCallbackService = bookingPaymentCallbackService;
            _linksProcessingService = linksProcessingService;
            _tagProcessor = tagProcessor;
            _context = context;
        }


        public async Task<Result<PaymentResponse>> ProcessCallback(JObject response)
        {
            var (_, isFailure, paymentResponse, error) = _responseParser.ParsePaymentResponse(response);
            if (isFailure)
                return Result.Failure<PaymentResponse>(error);

            var referenceCode = paymentResponse.ReferenceCode;
            // Reference code is retrieved from 'settlement_reference' parameter in Payfort payment data object.
            if (string.IsNullOrWhiteSpace(referenceCode))
                return Result.Failure<PaymentResponse>("Settlement reference cannot be empty");

            if (!_tagProcessor.IsCodeValid(referenceCode))
                return Result.Failure<PaymentResponse>("Invalid settlement reference");

            // We have no information about where this callback from: internal (authorized customer payment) or external (payment links).
            // So we'll try to process callback sequentially with different services and return first successful result (or fail).
            var internalPaymentProcessResult = await _creditCardPaymentProcessingService.ProcessPaymentResponse(response, _bookingPaymentCallbackService);
            if (internalPaymentProcessResult.IsSuccess)
                return internalPaymentProcessResult;

            // Trying to process callback with payment links.
            // TODO: refactor link services to process callbacks without link code (to correspond with interface of IPaymentService.
            var linkCode = await _context.PaymentLinks
                .Where(l => l.ReferenceCode == referenceCode)
                .Select(l => l.Code)
                .SingleOrDefaultAsync();

            if (linkCode == default)
                return Result.Failure<PaymentResponse>("Invalid settlement reference");

            return await _linksProcessingService.ProcessPayfortWebhook(linkCode, response);
        }


        private readonly EdoContext _context;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IPayfortResponseParser _responseParser;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
        private readonly IPaymentLinksProcessingService _linksProcessingService;

        private readonly ITagProcessor _tagProcessor;
    }
}