using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
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
        public NGeniusWebhookProcessingService(EdoContext context, ICreditCardPaymentManagementService paymentService,
            IPaymentLinksProcessingService paymentLinksProcessingService, ILogger<NGeniusWebhookProcessingService> logger)
        {
            _context = context;
            _paymentLinksProcessingService = paymentLinksProcessingService;
            _logger = logger;
            _paymentService = paymentService;
        }
        
        
        public async Task ProcessWebHook(NGeniusWebhookRequest request)
        {
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
                EventTypes.PartialRefundFailed => PaymentStatuses.Failed,
                EventTypes.ThreeDsNotAuthenticated => PaymentStatuses.Failed,
                _ => throw new System.NotImplementedException("")
            };

            using var disposable = _logger.BeginScope(new Dictionary<string, object>
            {
                {"ReferenceCode", paymentElement.MerchantOrderReference},
                {"EventType", eventType}
            });
            _logger.LogNGeniusWebhookProcessingStarted();

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
            
            // Payment not found
            if (payment is null)
                return;
            
            // Payment status not changed
            if (status == payment.Status)
                return;
            
            // Payment already captured
            if (status == PaymentStatuses.Authorized && payment.Status == PaymentStatuses.Captured)
                return;

            _logger.LogNGeniusWebhookPaymentUpdate();
            await _paymentService.SetStatus(payment, status);
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
            
            _logger.LogNGeniusWebhookPaymentLinkUpdate();
            await _paymentLinksProcessingService.ProcessNGeniusWebhook(paymentLink.Code, CreditCardPaymentStatuses.Success);
        }


        private readonly EdoContext _context;
        private readonly IPaymentLinksProcessingService _paymentLinksProcessingService;
        private readonly ILogger<NGeniusWebhookProcessingService> _logger;
        private readonly ICreditCardPaymentManagementService _paymentService;
    }
}