using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Data.PaymentLinks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static HappyTravel.MailSender.Formatters.EmailContentFormatter;
using MoneyFormatter = HappyTravel.Money.Helpers.PaymentAmountFormatter;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public class PaymentLinkService : IPaymentLinkService
    {
        public PaymentLinkService(IOptions<PaymentLinkOptions> options,
            MailSenderWithCompanyInfo mailSender,
            IPaymentLinksDocumentsService documentsService,
            IPaymentLinksStorage storage,
            ILogger<PaymentLinkService> logger)
        {
            _mailSender = mailSender;
            _documentsService = documentsService;
            _storage = storage;
            _logger = logger;
            _paymentLinkOptions = options.Value;
        }


        public Task<Result> Send(CreatePaymentLinkRequest paymentLinkData)
        {
            return CreateLink(paymentLinkData)
                .Bind(SendMail)
                .Finally(WriteLog);


            async Task<Result> SendMail(PaymentLinkData link)
            {
                var (registrationInfo, invoiceData) = await _documentsService.GetInvoice(link);
                
                var payload = new PaymentLinkInvoice
                {
                    Number = registrationInfo.Number,
                    Date = registrationInfo.Date,
                    Amount = MoneyFormatter.ToCurrencyString(invoiceData.Amount.Amount, invoiceData.Amount.Currency),
                    Comment = invoiceData.Comment,
                    PaymentLink = GeneratePaymentUri(link).ToString(),
                    ServiceDescription = FromEnumDescription(invoiceData.ServiceType)
                };

                return await _mailSender.Send(_paymentLinkOptions.MailTemplateId, paymentLinkData.Email, payload);
            }
            
            Result WriteLog(Result result)
            {
                if (result.IsFailure)
                    _logger.LogExternalPaymentLinkSendFailed($"Error sending email to {paymentLinkData.Email}: {result.Error}");
                else
                    _logger.LogExternalPaymentLinkSendSuccess($"Successfully sent e-mail to {paymentLinkData.Email}");

                return result;
            }
        }

        public Task<Result<Uri>> GenerateUri(CreatePaymentLinkRequest paymentLinkData)
        {
            return CreateLink(paymentLinkData)
                .Map(GeneratePaymentUri);
        }
        
        
        public ClientSettings GetClientSettings() => _paymentLinkOptions.ClientSettings;

        public List<Version> GetSupportedVersions() => _paymentLinkOptions.SupportedVersions;


        public Task<Result<PaymentLinkData>> Get(string code)
        {
            return _storage.Get(code)
                .Map(PaymentLinkExtensions.ToLinkData);
        }

        
        private Task<Result<PaymentLinkData>> CreateLink(CreatePaymentLinkRequest paymentLinkData)
        {
            return RegisterLink()
                .Tap(GenerateInvoice)
                .Map(PaymentLinkExtensions.ToLinkData)
                .Finally(WriteLog);

            
            Task<Result<PaymentLink>> RegisterLink() => _storage.Register(paymentLinkData);
            
            Task GenerateInvoice(PaymentLink link) => _documentsService.GenerateInvoice(link.ToLinkData());
            
            Result<PaymentLinkData> WriteLog(Result<PaymentLinkData> result)
            {
                if (result.IsFailure)
                    _logger.LogExternalPaymentLinkSendFailed($"Error generating payment link for {paymentLinkData.Email}: {result.Error}");
                else
                    _logger.LogExternalPaymentLinkSendSuccess($"Successfully generated payment link for {paymentLinkData.Email}");

                return result;
            }
        }


        private Uri GeneratePaymentUri(PaymentLinkData link) => new Uri($"{_paymentLinkOptions.PaymentUrlPrefix}/{link.Code}");

        private readonly ILogger<PaymentLinkService> _logger;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly PaymentLinkOptions _paymentLinkOptions;
        private readonly IPaymentLinksDocumentsService _documentsService;
        private readonly IPaymentLinksStorage _storage;
    }
}