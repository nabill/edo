using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Infrastructure.Emails;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.CodeGeneration;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinkService : IPaymentLinkService
    {
        public PaymentLinkService(EdoContext context,
            IOptions<PaymentLinkOptions> options,
            IMailSender mailSender, 
            IDateTimeProvider dateTimeProvider,
            IPayfortService payfortService,
            IJsonSerializer jsonSerializer,
            ITagGenerator tagGenerator,
            ILogger<PaymentLinkService> logger)
        {
            _context = context;
            _mailSender = mailSender;
            _dateTimeProvider = dateTimeProvider;
            _payfortService = payfortService;
            _jsonSerializer = jsonSerializer;
            _tagGenerator = tagGenerator;
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
                    v.RuleFor(data => data.ServiceType).IsInEnum();
                    v.RuleFor(data => data.Currency).IsInEnum();
                    v.RuleFor(data => data.Price).GreaterThan(decimal.Zero);
                    
                    v.RuleFor(data => data.Currency)
                        .Must(linkSettings.Currencies.Contains);
                    
                    v.RuleFor(data => data.ServiceType)
                        .Must(serviceType => linkSettings.ServiceTypes.Keys.Contains(serviceType));
                }, paymentLinkData);
            }


            string GenerateLinkCode() => Base64UrlEncoder.Encode(Guid.NewGuid().ToByteArray());

            async Task<Result<string>> SendMail(string code) 
            { 
                var (isSuccess, _, error) = await _mailSender.Send(_paymentLinkOptions.MailTemplateId, email,
                    new PaymentLinkMailData(code, paymentLinkData));

                return isSuccess
                    ? Result.Ok(code)
                    : Result.Fail<string>(error);
            }
            
            async Task StoreLink(string code)
            {
                var referenceCode = await  _tagGenerator.GenerateSingleReferenceCode(paymentLinkData.ServiceType, LinkDestinationCode);
                _context.PaymentLinks.Add(new PaymentLink
                {
                    Email = email,
                    Price = paymentLinkData.Price,
                    Currency = paymentLinkData.Currency,
                    ServiceType = paymentLinkData.ServiceType,
                    Comment = paymentLinkData.Comment,
                    Created = _dateTimeProvider.UtcNow(),
                    Code = code, 
                    ReferenceCode = referenceCode
                });
                await _context.SaveChangesAsync();
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
            return GetLink(code)
                .OnSuccess(ToLinkData);
            
            PaymentLinkData ToLinkData(PaymentLink link)
            {
                return new PaymentLinkData(link.Price,
                    link.ServiceType, 
                    link.Currency,
                    link.Comment);
            }
        }

        private Task<Result<PaymentLink>> GetLink(string code)
        {
            const string invalidCodeError = "Invalid link code";
            return ValidateCode()
                .OnSuccess(GetLinkData);
            
            Result ValidateCode()
            {
                if (code.Length != CodeLength)
                    return Result.Fail(invalidCodeError);

                var binaryData = Base64UrlEncoder.DecodeBytes(code);
                var isParsed = Guid.TryParse(BitConverter.ToString(binaryData).Replace("-", string.Empty),
                    out _);
                
                return isParsed ? Result.Ok() : Result.Fail(invalidCodeError);
            }
            
            async Task<Result<PaymentLink>> GetLinkData()
            {
                var link = await _context.PaymentLinks.SingleOrDefaultAsync(p => p.Code == code);
                if (link == default)
                    return Result.Fail<PaymentLink>(invalidCodeError);

                return Result.Ok(link);
            }
        }


        public Task<Result<PaymentResponse>> Pay(string code, string token, string ip, string languageCode)
        {
            return GetLink(code)
                .OnSuccess(Pay)
                .Map(ToPaymentResponse);

            Task<Result<CreditCardPaymentResult>> Pay(PaymentLink link)
            {
                return _payfortService.Pay(new CreditCardPaymentRequest(
                    amount: link.Price,
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

            async Task<Result<PaymentLink>> GetLinkToPay()
            {
                var (_, isFailure, link, error) = await GetLink(code);
                if (isFailure)
                    return Result.Fail<PaymentLink>(error);
                
                if(string.IsNullOrWhiteSpace(link.LastPaymentResponse))
                    return Result.Ok(link);

                var lastPaymentResponse = _jsonSerializer.DeserializeObject<PaymentResponse>(link.LastPaymentResponse);
                return lastPaymentResponse.Status != PaymentStatuses.Success
                    ? Result.Ok(link)
                    : Result.Fail<PaymentLink>("Link is already paid");
            } 
                    
            Result<(PaymentLink, PaymentResponse)> ProcessCardResponse(PaymentLink link)
            {
                var (_, isFailure, cardPaymentResult, error) = _payfortService.ProcessPaymentResponse(response);
                if (isFailure)
                    return Result.Fail<(PaymentLink, PaymentResponse)>(error);

                var paymentResponse = new PaymentResponse(cardPaymentResult.Message, cardPaymentResult.Status);
                
                return Result.Ok((link, paymentResponse));
            }

            async Task<PaymentResponse> StorePaymentResult((PaymentLink Link, PaymentResponse Response) paymentData)
            {
                var (link, paymentResponse) = paymentData;
                link.LastPaymentDate = _dateTimeProvider.UtcNow();
                link.LastPaymentResponse = _jsonSerializer.SerializeObject(paymentResponse);
                
                _context.Update(link);
                await _context.SaveChangesAsync();
                return paymentResponse;
            }
        }


        private static readonly int CodeLength = Base64UrlEncoder.Encode(Guid.Empty.ToByteArray()).Length;
        private const string LinkDestinationCode = "LNK";
        private readonly EdoContext _context;
        private readonly IMailSender _mailSender;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPayfortService _payfortService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITagGenerator _tagGenerator;
        private readonly ILogger<PaymentLinkService> _logger;
        private readonly PaymentLinkOptions _paymentLinkOptions;
    }
}