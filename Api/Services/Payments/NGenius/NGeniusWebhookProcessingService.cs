using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
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
        
        
        public async Task ProcessWebHook(NGeniusWebhookRequest request)
        {
            _logger.LogDebug("NGenius webhook processing started. Request `{Request}`", request);

            var eventType = request.EventName;
            var paymentElement = request.Order.Embedded.Payments[0];
            var paymentId = paymentElement.Id.Split(':').Last();
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

            await TryUpdatePayment(paymentElement.MerchantOrderReference, paymentId, status);
            await TryUpdatePaymentLink(paymentElement.MerchantOrderReference, status);
        }


        private async Task TryUpdatePayment(string referenceCode, string paymentId, PaymentStatuses status)
        {
            var payments = await _context.Payments
                .Where(p => p.ReferenceCode == referenceCode)
                .ToListAsync();

            var payment = payments.Where(p =>
                {
                    var data = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(p.Data);
                    return data.ExternalId == paymentId;
                })
                .SingleOrDefault();

            if (payment is null)
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


        private async Task TryUpdatePaymentLink(string referenceCode, PaymentStatuses status)
        {
            var paymentLink = await _context.PaymentLinks
                .Where(l => l.ReferenceCode == referenceCode)
                .SingleOrDefaultAsync();
            
            if (paymentLink is null)
                return;
            
            // Only Captured status processed
            if (status is not PaymentStatuses.Captured)
                return;
            
            await _paymentLinksProcessingService.ProcessNGeniusWebhook(paymentLink.Code, CreditCardPaymentStatuses.Success);
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
        private readonly IPaymentLinksProcessingService _paymentLinksProcessingService;
        private readonly ILogger<NGeniusWebhookProcessingService> _logger;
    }
}