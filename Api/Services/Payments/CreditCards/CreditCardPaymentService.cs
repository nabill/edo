using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CreditCardPaymentRequest = HappyTravel.Edo.Api.Models.Payments.CreditCardPaymentRequest;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardPaymentProcessingService : ICreditCardPaymentProcessingService
    {
        public CreditCardPaymentProcessingService(IPayfortResponseParser responseParser,
            EdoContext context,
            ICreditCardsManagementService creditCardsManagementService,
            IEntityLocker locker,
            IDateTimeProvider dateTimeProvider,
            ICreditCardMoneyAuthorizationService moneyAuthorizationService,
            ICreditCardMoneyCaptureService captureService,
            ICreditCardMoneyRefundService refundService, 
            ILogger<CreditCardPaymentProcessingService> logger)
        {
            _responseParser = responseParser;
            _context = context;
            _creditCardsManagementService = creditCardsManagementService;
            _locker = locker;
            _dateTimeProvider = dateTimeProvider;
            _moneyAuthorizationService = moneyAuthorizationService;
            _captureService = captureService;
            _refundService = refundService;
            _logger = logger;
        }
        
        
        public async Task<Result<PaymentResponse>> Authorize(CreditCardPaymentRequest request, 
            string languageCode, string ipAddress, IPaymentCallbackService paymentCallbackService, AgentContext agent)
        {
            _logger.LogCreditCardAuthorizationStarted(request.ReferenceCode);
            
            var (_, isFailure, servicePrice, error) = await paymentCallbackService.GetChargingAmount(request.ReferenceCode);
            if (isFailure)
            {
                _logger.LogCreditCardAuthorizationFailure(request.ReferenceCode, error);
                return Result.Failure<PaymentResponse>(error);
            }

            return await Authorize()
                .Bind(StorePaymentResults)
                .Finally(WriteLog);

            async Task<Result<CreditCardPaymentResult>> Authorize()
            {
                var cardPaymentRequest = await CreatePaymentRequest(servicePrice,
                    new PaymentTokenInfo(request.Token, PaymentTokenTypes.OneTime),
                    ipAddress, request.ReferenceCode,
                    languageCode, agent);

                return await _moneyAuthorizationService.AuthorizeMoneyForService(cardPaymentRequest, agent);
            }
            
            
            async Task<Result<PaymentResponse>> StorePaymentResults(CreditCardPaymentResult paymentResult)
            {
                var payment = await CreatePayment(ipAddress, servicePrice, null, paymentResult);
                var (_, isFailure, error) = await paymentCallbackService.ProcessPaymentChanges(payment);

                return isFailure
                    ? Result.Failure<PaymentResponse>(error)
                    : Result.Success(paymentResult.ToPaymentResponse());
            }


            Result<PaymentResponse> WriteLog(Result<PaymentResponse> result)
            {
                return LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogCreditCardAuthorizationSuccess(request.ReferenceCode),
                    () => _logger.LogCreditCardAuthorizationFailure(request.ReferenceCode, result.Error));
            }
        }
        

        private async Task<Payment> CreatePayment(string ipAddress, MoneyAmount moneyAmount,
            int? cardId, CreditCardPaymentResult paymentResult)
        {
            var now = _dateTimeProvider.UtcNow();
            var info = new CreditCardPaymentInfo(ipAddress, 
                paymentResult.ExternalCode,
                paymentResult.Message, 
                paymentResult.AuthorizationCode, 
                paymentResult.ExpirationDate,
                paymentResult.MerchantReference);

            var payment = new Payment
            {
                Amount = paymentResult.Amount,
                AccountNumber = paymentResult.CardNumber,
                Currency = moneyAmount.Currency,
                Created = now,
                Modified = now,
                Status = paymentResult.Status.ToPaymentStatus(),
                Data = JsonConvert.SerializeObject(info),
                AccountId = cardId,
                PaymentMethod = PaymentTypes.CreditCard,
                PaymentProcessor = PaymentProcessors.Payfort,
                ReferenceCode = paymentResult.ReferenceCode
            };
                
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }


        private async Task<Models.Payments.Payfort.CreditCardPaymentRequest> CreatePaymentRequest(MoneyAmount moneyAmount,
            PaymentTokenInfo paymentToken,
            string ipAddress,
            string referenceCode,
            string languageCode,
            AgentContext agent,
            string securityCode = default)
        {
            return new Models.Payments.Payfort.CreditCardPaymentRequest(currency: moneyAmount.Currency,
                amount: moneyAmount.Amount,
                token: paymentToken,
                customerName: $"{agent.FirstName} {agent.LastName}",
                customerEmail: agent.Email,
                customerIp: ipAddress,
                referenceCode: referenceCode,
                languageCode: languageCode,
                securityCode: securityCode,
                isNewCard: string.IsNullOrWhiteSpace(securityCode),
                merchantReference: await GetMerchantReference());


            async Task<string> GetMerchantReference()
            {
                var count = await _context.Payments.Where(p => p.ReferenceCode == referenceCode).CountAsync();
                return count == 0
                    ? referenceCode
                    : $"{referenceCode}-{count}";
            }
        }
        
        
        public async Task<Result<PaymentResponse>> ProcessPaymentResponse(JObject rawResponse, IPaymentCallbackService paymentCallbackService)
        {
            _logger.LogCreditCardProcessingPaymentStarted();
            
            var (_, isParseFailure, paymentResponse, parseError) = _responseParser.ParsePaymentResponse(rawResponse);
            if (isParseFailure)
            {
                _logger.LogCreditCardProcessingPaymentFailure(parseError);
                return Result.Failure<PaymentResponse>(parseError);
            }

            return await Result.Success()
                .BindWithLock(_locker, typeof(Payment), paymentResponse.ReferenceCode, () => Result.Success()
                    .Bind(ProcessPaymentResponse)
                    .Map(StorePaymentResults))
                .Finally(WriteLog);
            
            async Task<Result<(CreditCardPaymentResult, Payment)>> ProcessPaymentResponse()
            {
                var (_, isFailure, payment, error) = await GetPaymentForResponse(paymentResponse);
                if (isFailure)
                    return Result.Failure<(CreditCardPaymentResult, Payment)>(error);
                
                // Payment can be completed before. Nothing to do now.
                if (payment.Status == PaymentStatuses.Authorized)
                    return Result.Success((GetDefaultSuccessResult(), payment));

                var (_, _, serviceBuyer, _) = await paymentCallbackService.GetServiceBuyer(payment.ReferenceCode);
                
                var (_, isProcessFailure, processResult, processError) = await _moneyAuthorizationService.ProcessPaymentResponse(paymentResponse,
                    payment.Currency,
                    serviceBuyer.AgentId);
                
                if (isProcessFailure)
                    return Result.Failure<(CreditCardPaymentResult, Payment)>(processError);
                
                return Result.Success((processResult, payment));
                
                static CreditCardPaymentResult GetDefaultSuccessResult() => new CreditCardPaymentResult(string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    CreditCardPaymentStatuses.Success,
                    string.Empty,
                    default,
                    string.Empty);
            }
            
            
            async Task<PaymentResponse> StorePaymentResults((CreditCardPaymentResult, Payment) result)
            {
                var (paymentResult, payment) = result;
                // Payment can be completed before. Nothing to do now.
                if (payment.Status == PaymentStatuses.Authorized)
                    return paymentResult.ToPaymentResponse();
                
                var info = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
                var newInfo = new CreditCardPaymentInfo(info.CustomerIp, paymentResult.ExternalCode, paymentResult.Message, paymentResult.AuthorizationCode,
                    paymentResult.ExpirationDate, info.InternalReferenceCode);
                payment.Status = paymentResult.Status.ToPaymentStatus();
                payment.Data = JsonConvert.SerializeObject(newInfo);
                payment.Modified = _dateTimeProvider.UtcNow();
                _context.Update(payment);
                await _context.SaveChangesAsync();
                await paymentCallbackService.ProcessPaymentChanges(payment);

                return paymentResult.ToPaymentResponse();
            }


            async Task<Result<Payment>> GetPaymentForResponse(CreditCardPaymentResult paymentResponse)
            {
                var cardPaymentsForRefcode = await _context.Payments
                    .Where(p => p.PaymentMethod == PaymentTypes.CreditCard)
                    .Where(p => p.ReferenceCode == paymentResponse.ReferenceCode)
                    .ToListAsync();

                var payment = cardPaymentsForRefcode
                    .SingleOrDefault(p => JsonConvert.DeserializeObject<CreditCardPaymentInfo>(p.Data).ExternalId == paymentResponse.ExternalCode);

                if (payment == default)
                    return Result.Failure<Payment>($"Could not find a payment record with the reference code {paymentResponse.ReferenceCode}");

                if (payment.Amount != paymentResponse.Amount)
                    return Result.Failure<Payment>($"Invalid payment amount, expected: '{payment.Amount}', actual: '{paymentResponse.Amount}'");
                
                return Result.Success(payment);
            }


            Result<PaymentResponse> WriteLog(Result<PaymentResponse> paymentResponseResult)
            {
                return LoggerUtils.WriteLogByResult(paymentResponseResult,
                    () => _logger.LogCreditCardProcessingPaymentSuccess(),
                    () => _logger.LogCreditCardProcessingPaymentFailure(paymentResponseResult.Error));
            }
        }
        
        
        public async Task<Result<string>> CaptureMoney(string referenceCode, ApiCaller apiCaller, IPaymentCallbackService paymentCallbackService)
        {
            _logger.LogCreditCardCapturingStarted(referenceCode);

            var (_, isFailure, payment, error) = await GetPayment();
            if (isFailure)
            {
                _logger.LogCreditCardCapturingFailure(referenceCode, error);
                return Result.Failure<string>(error);
            }

            return await Capture()
                .Tap(StoreCaptureResults)
                .Finally(CreateResult);
            
            
            // TODO: use service instead of dbContext
            async Task<Result<Payment>> GetPayment()
            {
                var payment = await _context.Payments
                    .Where(p => p.Status == PaymentStatuses.Authorized)
                    .SingleOrDefaultAsync(p => p.ReferenceCode == referenceCode);

                if (payment == default)
                    return Result.Failure<Payment>($"Could not find payment by reference code {referenceCode}");
            
                if (payment.Status != PaymentStatuses.Authorized)
                    return Result.Failure<Payment>($"Not supported payment status for payment with reference code {referenceCode}");
            
                if (payment.PaymentMethod != PaymentTypes.CreditCard)
                    return Result.Failure<Payment>($"Invalid payment method: {payment.PaymentMethod}");

                return payment;
            }
            

            async Task<Result<CreditCardCaptureResult>> Capture()
            {
                var (_, isFailure, servicePrice, error) = await paymentCallbackService.GetChargingAmount(referenceCode);
                if (isFailure)
                    return Result.Failure<CreditCardCaptureResult>(error);
                
                var paymentInfo = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);

                var request = new CreditCardCaptureMoneyRequest(currency: servicePrice.Currency,
                    amount: servicePrice.Amount,
                    externalId: paymentInfo.ExternalId,
                    merchantReference: paymentInfo.InternalReferenceCode,
                    languageCode: "en");

                var (_, _, buyerInfo, _) = await paymentCallbackService.GetServiceBuyer(referenceCode);

                return await _captureService.Capture(request,
                    paymentInfo,
                    payment.PaymentProcessor,
                    payment.AccountNumber,
                    payment.Currency,
                    apiCaller,
                    buyerInfo.AgentId);
            }
            
            
            async Task StoreCaptureResults(CreditCardCaptureResult captureResult)
            {
                payment.Status = PaymentStatuses.Captured;
                payment.CaptureId = captureResult.CaptureId;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                await paymentCallbackService.ProcessPaymentChanges(payment);
            }

            
            Result<string> CreateResult(Result<CreditCardCaptureResult> result)
            {
                if (result.IsSuccess)
                {
                    _logger.LogCreditCardCapturingSuccess(referenceCode);
                    return Result.Success($"Payment for the payment '{payment.ReferenceCode}' completed.");
                }
                    
                _logger.LogCreditCardCapturingFailure(referenceCode, result.Error);
                return Result.Failure<string>($"Unable to complete payment for the payment '{payment.ReferenceCode}'. Reason: {result.Error}");
            }
        }
        
        
        public async Task<Result> VoidMoney(string referenceCode, ApiCaller apiCaller, IPaymentCallbackService paymentCallbackService)
        {
            _logger.LogCreditCardVoidingStarted(referenceCode);

            var (_, isFailure, payment, error) = await GetPayment();
            if (isFailure)
            {
                _logger.LogCreditCardVoidingFailure(referenceCode, error);
                return Result.Failure(error);
            }

            return await Void()
                .Tap(StoreVoidResults)
                .Finally(WriteLog);


            // TODO: use service instead of dbContext
            async Task<Result<Payment>> GetPayment()
            {
                var payment = await _context.Payments
                    .SingleOrDefaultAsync(p => p.ReferenceCode == referenceCode && p.Status == PaymentStatuses.Authorized);
            
                if (payment == default)
                    return Result.Failure<Payment>($"Could not find payment by reference code {referenceCode}");

                return payment;
            }

            async Task<Result<CreditCardVoidResult>> Void()
            {
                var info = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
                var request = new CreditCardVoidMoneyRequest(
                    externalId: info.ExternalId,
                    merchantReference: info.InternalReferenceCode,
                    languageCode: "en");

                var (_, _, buyerInfo, _) = await paymentCallbackService.GetServiceBuyer(referenceCode);

                return await _captureService.Void(request,
                    info,
                    payment.PaymentProcessor,
                    payment.AccountNumber,
                    new MoneyAmount(payment.Amount, payment.Currency),
                    payment.ReferenceCode,
                    apiCaller,
                    buyerInfo.AgentId);
            }
            
            async Task StoreVoidResults(CreditCardVoidResult voidResult)
            {
                payment.Status = PaymentStatuses.Voided;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                await paymentCallbackService.ProcessPaymentChanges(payment);
            }
            
            
            Result WriteLog(Result<CreditCardVoidResult> result)
            {
                return LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogCreditCardVoidingSuccess(referenceCode),
                    () => _logger.LogCreditCardVoidingFailure(referenceCode, result.Error));
            }
        }


        public async Task<Result> RefundMoney(string referenceCode, ApiCaller apiCaller, DateTime operationDate,  IPaymentCallbackService paymentCallbackService)
        {
            _logger.LogCreditCardRefundingStarted(referenceCode);
            
            var (_, isFailure, payment, error) = await GetPayment();
            if (isFailure)
            {
                _logger.LogCreditCardRefundingFailure(referenceCode, error);
                return Result.Failure(error);
            }

            return await GetRefundableAmount()
                .Bind(Refund)
                .Bind(StoreRefundResults)
                .Finally(WriteLog);


            async Task<Result<Payment>> GetPayment()
            {
                var payment = await _context.Payments
                    .SingleOrDefaultAsync(p => p.ReferenceCode == referenceCode && p.Status == PaymentStatuses.Captured);
            
                if (payment == default)
                    return Result.Failure<Payment>($"Could not find payment by reference code {referenceCode}");

                return payment;
            }


            Task<Result<MoneyAmount>> GetRefundableAmount() 
                => paymentCallbackService.GetRefundableAmount(referenceCode, operationDate);


            async Task<Result<CreditCardRefundResult>> Refund(MoneyAmount refundingAmount)
            {
                var info = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
                var request = new CreditCardRefundMoneyRequest(
                    currency: refundingAmount.Currency,
                    amount: refundingAmount.Amount,
                    externalId: info.ExternalId,
                    merchantReference: info.InternalReferenceCode,
                    languageCode: "en");

                var (_, _, buyerInfo, _) = await paymentCallbackService.GetServiceBuyer(referenceCode);

                return await _refundService.Refund(request,
                    info,
                    payment.PaymentProcessor,
                    payment.AccountNumber,
                    payment.ReferenceCode,
                    payment.Id,
                    apiCaller,
                    buyerInfo.AgentId);
            }


            async Task<Result> StoreRefundResults(CreditCardRefundResult result)
            {
                payment.Status = PaymentStatuses.Refunded;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                await paymentCallbackService.ProcessPaymentChanges(payment);
                return Result.Success();
            }
            
            
            Result WriteLog(Result result)
            {
                return LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogCreditCardRefundingSuccess(referenceCode),
                    () => _logger.LogCreditCardRefundingFailure(referenceCode, result.Error));
            }
        }


        private readonly IPayfortResponseParser _responseParser;
        private readonly EdoContext _context;
        private readonly ICreditCardsManagementService _creditCardsManagementService;
        private readonly IEntityLocker _locker;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICreditCardMoneyAuthorizationService _moneyAuthorizationService;
        private readonly ICreditCardMoneyCaptureService _captureService;
        private readonly ICreditCardMoneyRefundService _refundService;
        private readonly ILogger<CreditCardPaymentProcessingService> _logger;
    }
}