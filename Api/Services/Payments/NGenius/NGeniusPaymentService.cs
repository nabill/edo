using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusPaymentService : INGeniusPaymentService
    {
        public NGeniusPaymentService(EdoContext context, IDateTimeProvider dateTimeProvider, IBookingRecordManager bookingRecordManager, INGeniusClient client, 
            IBookingPaymentCallbackService bookingPaymentCallbackService, IAgencyService agencyService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _bookingRecordManager = bookingRecordManager;
            _client = client;
            _bookingPaymentCallbackService = bookingPaymentCallbackService;
            _agencyService = agencyService;
        }


        public async Task<Result<NGeniusPaymentResponse>> Authorize(string referenceCode, string ipAddress, AgentContext agent)
        {
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<NGeniusPaymentResponse>(error);

            var agency = await _agencyService.Get(agent);
            if (agency.IsFailure)
                return Result.Failure<NGeniusPaymentResponse>(agency.Error);

            var billingAddress = GetBillingAddress(agent, agency.Value);

            return await _client.CreateOrder(orderType: OrderTypes.Auth,
                    referenceCode: booking.ReferenceCode,
                    currency: booking.Currency,
                    price: booking.TotalPrice,
                    email: agent.Email,
                    billingAddress: billingAddress)
                .Bind(r => StorePaymentResults(ipAddress, booking.TotalPrice.ToMoneyAmount(booking.Currency), null, r));
        }


        public async Task<Result<CreditCardCaptureResult>> Capture(string paymentId, string orderReference, MoneyAmount amount)
        {
            var result = await _client.CaptureMoney(paymentId, orderReference, amount);

            return result.IsFailure 
                ? Result.Failure<CreditCardCaptureResult>(result.Error) 
                : new CreditCardCaptureResult(paymentId, string.Empty, orderReference, result.Value);
        }


        public async Task<Result<CreditCardVoidResult>> Void(string paymentId, string orderReference)
        {
            var result = await _client.VoidMoney(paymentId, orderReference);
            return result.IsFailure
                ? Result.Failure<CreditCardVoidResult>(result.Error)
                : new CreditCardVoidResult(paymentId, string.Empty, orderReference);
        }


        public async Task<Result<CreditCardRefundResult>> Refund(string paymentId, string orderReference, string captureId, MoneyAmount amount)
        {
            var result = await _client.RefundMoney(paymentId, orderReference, captureId, amount);
            
            return result.IsFailure
                ? Result.Failure<CreditCardRefundResult>(result.Error)
                : new CreditCardRefundResult(paymentId, string.Empty, orderReference);
        }


        public async Task<Result<StatusResponse>> RefreshStatus(string referenceCode)
        {
            var payment = await _context.Payments
                .OrderByDescending(p => p.Created)
                .FirstOrDefaultAsync(p => p.PaymentProcessor == PaymentProcessors.NGenius && p.ReferenceCode == referenceCode);

            if (payment is null)
                return Result.Failure<StatusResponse>($"Payment for {referenceCode} not found");

            var data = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
            var (_, isFailure, status) = await _client.GetStatus(data.InternalReferenceCode);

            if (isFailure)
                return Result.Failure<StatusResponse>("Status checking failed");

            if (payment.Status != status)
            {
                payment.Status = status;
                payment.Modified = _dateTimeProvider.UtcNow();
                _context.Update(payment);
                await _context.SaveChangesAsync();
                await _bookingPaymentCallbackService.ProcessPaymentChanges(payment);
            }
            
            return new StatusResponse(payment.Status == PaymentStatuses.Authorized
                ? CreditCardPaymentStatuses.Success
                : CreditCardPaymentStatuses.Failed);
        }


        private async Task<Edo.Data.Payments.Payment> CreatePayment(string ipAddress, MoneyAmount price, int? cardId, NGeniusPaymentResponse paymentResult)
        {
            var now = _dateTimeProvider.UtcNow();

            var info = new CreditCardPaymentInfo(customerIp: ipAddress, 
                externalId: paymentResult.PaymentId,
                message: string.Empty, 
                authorizationCode: string.Empty, 
                expirationDate: string.Empty,
                internalReferenceCode: paymentResult.OrderReference);

            var payment = new Edo.Data.Payments.Payment
            {
                Amount = price.Amount,
                Currency = price.Currency.ToString(),
                AccountNumber = string.Empty,
                Created = now,
                Modified = now,
                Status = PaymentStatuses.Created,
                Data = JsonConvert.SerializeObject(info),
                AccountId = cardId,
                PaymentMethod = PaymentTypes.CreditCard,
                PaymentProcessor = PaymentProcessors.NGenius,
                ReferenceCode = paymentResult.MerchantOrderReference
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
        
        
        private async Task<Result<NGeniusPaymentResponse>> StorePaymentResults(string ipAddress, MoneyAmount price, int? cardId, NGeniusPaymentResponse paymentResult)
        {
            var payment = await CreatePayment(ipAddress, price, cardId, paymentResult);
            var (_, isFailure, error) = await _bookingPaymentCallbackService.ProcessPaymentChanges(payment);

            return isFailure
                ? Result.Failure<NGeniusPaymentResponse>(error)
                : Result.Success(paymentResult);
        }


        private static NGeniusBillingAddress GetBillingAddress(AgentContext agent, SlimAgencyInfo agency)
            => new ()
            {
                FirstName = agent.FirstName,
                LastName = agent.LastName,
                Address1 = agency.Address,
                City = agency.City,
                CountryCode = agency.CountryCode
            };
        
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly INGeniusClient _client;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
        private readonly IAgencyService _agencyService;
    }
}