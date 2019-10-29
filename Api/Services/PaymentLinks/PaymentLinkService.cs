using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Emails;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments.External;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinkService : IPaymentLinkService
    {
        public PaymentLinkService(EdoContext context,
            IOptions<PaymentLinkOptions> options,
            IMailSender mailSender, 
            IDateTimeProvider dateTimeProvider,
            ILogger<PaymentLinkService> logger)
        {
            _context = context;
            _mailSender = mailSender;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _paymentLinkOptions = options.Value;
        }
        
        public Task<Result> Send(string email, PaymentLinkData paymentLinkData)
        {
            return ValidateEmail()
                .OnSuccess(ValidatePaymentData)
                .OnSuccess(GenerateLinkCode)
                .OnSuccess(SendMail)
                .OnSuccess(StoreLink)
                .OnBoth(WriteLog);

            Result ValidateEmail()
            {
                return GenericValidator<string>.Validate(v =>
                {
                    v.RuleFor(mail => mail).NotEmpty();
                    v.RuleFor(mail => mail).EmailAddress();
                }, email);
            }

            Result ValidatePaymentData()
            {
                var linkSettings = _paymentLinkOptions.ClientSettings;
                return GenericValidator<PaymentLinkData>.Validate(v =>
                {
                    v.RuleFor(data => data.Facility).NotEmpty();
                    v.RuleFor(data => data.Currency).IsInEnum();
                    v.RuleFor(data => data.Price).GreaterThan(decimal.Zero);
                    
                    v.RuleFor(data => data.Currency)
                        .Must(linkSettings.Currencies.Contains);
                    
                    v.RuleFor(data => data.Facility)
                        .Must(linkSettings.Facilities.Contains);
                }, paymentLinkData);
            }


            string GenerateLinkCode() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            async Task<Result<string>> SendMail(string code) 
            { 
                var (isSuccess, _, error) = await _mailSender.Send(_paymentLinkOptions.MailTemplateId, email,
                    new PaymentLinkMailData(code, paymentLinkData));

                return isSuccess
                    ? Result.Ok(code)
                    : Result.Fail<string>(error);
            }

            Task StoreLink(string code)
            {
                _context.PaymentLinks.Add(new PaymentLink
                {
                    Email = email,
                    Price = paymentLinkData.Price,
                    Currency = paymentLinkData.Currency,
                    Facility = paymentLinkData.Facility,
                    Comment = paymentLinkData.Comment,
                    IsPaid = false,
                    Created = _dateTimeProvider.UtcNow(),
                    Code = code
                });
                return _context.SaveChangesAsync();
            }

            Result WriteLog(Result<string> result)
            {
                if (result.IsFailure)
                    _logger.LogExternalPaymentLinkSendFailed($"Error sending email to {email}: {result.Error}");
                else
                    _logger.LogExternalPaymentLinkSendSuccess($"Successfully sent e-mail to {email}");

                return result;
            }
        }


        public ClientSettings GetClientSettings() => _paymentLinkOptions.ClientSettings;

        public List<Version> GetSupportedVersions() => _paymentLinkOptions.SupportedVersions;

        public Task<Result<PaymentLinkData>> Get(string code)
        {
            return Result.Ok()
                .Ensure(CodeIsValid, "Invalid link code")
                .OnSuccess(GetLinkData)
                .Map(ToLinkData);
            
            bool CodeIsValid()
            {
                if (code.Length != CodeLength)
                    return false;

                var binaryData = Convert.FromBase64String(code);
                return Guid.TryParse(
                    BitConverter.ToString(binaryData).Replace("-", string.Empty),
                    out _);
            }
            
            async Task<Result<PaymentLink>> GetLinkData()
            {
                var link = await _context.PaymentLinks.SingleOrDefaultAsync(p => p.Code == code);
                if (link == default)
                    return Result.Fail<PaymentLink>("Invalid link code");

                return Result.Ok(link);
            }
            
            PaymentLinkData ToLinkData(PaymentLink link)
            {
                return new PaymentLinkData(link.Price,
                    link.Facility, 
                    link.Currency,
                    link.Comment);
            }
        }

        private static readonly int CodeLength = Convert.ToBase64String(Guid.Empty.ToByteArray()).Length;
        private readonly EdoContext _context;
        private readonly IMailSender _mailSender;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<PaymentLinkService> _logger;
        private readonly PaymentLinkOptions _paymentLinkOptions;
    }
}