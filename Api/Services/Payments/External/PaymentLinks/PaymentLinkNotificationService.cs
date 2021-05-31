using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.Extensions.Options;
using MoneyFormatter = HappyTravel.DataFormatters.MoneyFormatter;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public class PaymentLinkNotificationService : IPaymentLinkNotificationService
    {
        public PaymentLinkNotificationService(IOptions<PaymentLinkOptions> options, INotificationService notificationService)
        {
            _options = options.Value;
            _notificationService = notificationService;
        }


        public Task<Result> SendLink(PaymentLinkData link, string paymentUrl)
        {
            var payload = new PaymentLinkMail
            {
                Amount = MoneyFormatter.ToCurrencyString(link.Amount, link.Currency),
                Comment = link.Comment,
                ServiceDescription = link.ServiceType.ToString(),
                ReferenceCode = link.ReferenceCode,
                PaymentLink = paymentUrl
            };

            return _notificationService.Send(messageData: payload,
                notificationType: NotificationTypes.ExternalPaymentLinks,
                email: link.Email,
                templateId: _options.LinkMailTemplateId);
        }


        public Task<Result> SendPaymentConfirmation(PaymentLinkData link)
        {
            var payload = new PaymentLinkPaymentConfirmation
            {
                Date = FormatDate(link.Date),
                Amount = MoneyFormatter.ToCurrencyString(link.Amount, link.Currency),
                ReferenceCode = link.ReferenceCode,
                ServiceDescription = link.ServiceType.ToString(),
            };

            return _notificationService.Send(messageData: payload,
                notificationType: NotificationTypes.PaymentLinkPaidNotification,
                email: link.Email,
                templateId: _options.PaymentConfirmationMailTemplateId);
        }
        
        
        private static string FormatDate(DateTime date) => date.ToString("dd-MMM-yy");
        
        
        private readonly PaymentLinkOptions _options;
        private readonly INotificationService _notificationService;
    }
}