using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusWebhookProcessingService
    {
        public NGeniusWebhookProcessingService(EdoContext context, IDateTimeProvider dateTimeProvider, IBookingPaymentCallbackService bookingPaymentCallbackService,
            IPaymentLinksProcessingService paymentLinksProcessingService, ILogger<NGeniusWebhookProcessingService> logger)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _bookingPaymentCallbackService = bookingPaymentCallbackService;
            _paymentLinksProcessingService = paymentLinksProcessingService;
            _logger = logger;
        }
        
        
        public async Task ProcessWebHook(JsonDocument request)
        {
            _logger.LogDebug("NGenius webhook processing started");
            
            var eventType = request.RootElement.GetProperty("eventName").GetString();
            var paymentElement = request.RootElement.GetProperty("_embedded").GetProperty("payment")[0];
            var paymentId = paymentElement.GetProperty("_id").GetString().Split(':').Last();
            var orderReference = paymentElement.GetProperty("orderReference").GetString();
            var merchantReference = paymentElement.GetProperty("merchantOrderReference").GetString();
            var status = eventType switch
            {
                EventTypes.Authorised => PaymentStatuses.Authorized,
                EventTypes.Captured => PaymentStatuses.Captured,
                EventTypes.FullAuthReversed => PaymentStatuses.Voided,
                EventTypes.Refunded => PaymentStatuses.Refunded,
                EventTypes.PartiallyRefunded => PaymentStatuses.Refunded,
                EventTypes.Declined => PaymentStatuses.Failed,
                EventTypes.AuthorisationFailed => PaymentStatuses.Failed,
                EventTypes.CaptureFailed => PaymentStatuses.Failed,
                EventTypes.FullAuthReversalFailed => PaymentStatuses.Failed,
                EventTypes.RefundFailed => PaymentStatuses.Failed,
                EventTypes.PartialRefundFailed => PaymentStatuses.Failed
            };

            var payments = await _context.Payments
                .Where(p => p.ReferenceCode == merchantReference)
                .ToListAsync();

            var payment = payments.Where(p =>
                {
                    var data = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(p.Data);
                    return data.ExternalId == paymentId && data.InternalReferenceCode == orderReference;
                })
                .SingleOrDefault();

            if (payment != default)
            {
                // It is important to note that, for orders processed in SALE mode
                // (whereby a transaction is AUTHORISED and then immediately CAPTURED, ready for settlement)
                // your nominated URL will receive multiple web-hooks at the same - one for the AUTHORISED event,
                // and another for the CAPTURED event.
                if (payment.Status == PaymentStatuses.Captured && status == PaymentStatuses.Authorized)
                    return;

                if (status != payment.Status)
                {
                    payment.Status = status;
                    payment.Modified = _dateTimeProvider.UtcNow();
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    await _bookingPaymentCallbackService.ProcessPaymentChanges(payment);
                }
            }
            
            var paymentLink = await _context.PaymentLinks
                .Where(l => l.ReferenceCode == merchantReference)
                .SingleOrDefaultAsync();

            if (paymentLink != default && status is PaymentStatuses.Captured or PaymentStatuses.Authorized)
            {
                await _paymentLinksProcessingService.ProcessNGeniusWebhook(paymentLink.Code, CreditCardPaymentStatuses.Success);
            }
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
        private readonly IPaymentLinksProcessingService _paymentLinksProcessingService;
        private readonly ILogger<NGeniusWebhookProcessingService> _logger;
    }
}