using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
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
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusPaymentService : INGeniusPaymentService
    {
        public NGeniusPaymentService(EdoContext context, IDateTimeProvider dateTimeProvider, IBookingRecordManager bookingRecordManager, 
            INGeniusClient client, IBookingPaymentCallbackService bookingPaymentCallbackService, IAgencyService agencyService)
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

            return await _agencyService.Get(agent)
                .Bind(CreateOrder)
                .Bind(StorePayment);


            Task<Result<NGeniusPaymentResponse>> CreateOrder(SlimAgencyInfo agency)
                => _client.CreateOrder(orderType: OrderTypes.Auth,
                    referenceCode: booking.ReferenceCode,
                    currency: booking.Currency,
                    price: booking.TotalPrice,
                    email: agent.Email,
                    billingAddress: (agent, agency).ToBillingAddress());


            Task<Result<NGeniusPaymentResponse>> StorePayment(NGeniusPaymentResponse response)
                => StorePaymentResults(ipAddress: ipAddress, 
                    price: booking.TotalPrice.ToMoneyAmount(booking.Currency), 
                    paymentResult: response);
        }


        public Task<Result<CreditCardCaptureResult>> Capture(string paymentId, string orderReference, MoneyAmount amount)
            => _client.CaptureMoney(paymentId, orderReference, amount);


        public Task<Result<CreditCardVoidResult>> Void(string paymentId, string orderReference, Currencies currency) 
            => _client.VoidMoney(paymentId, orderReference, currency);


        public Task<Result<CreditCardRefundResult>> Refund(string paymentId, string orderReference, string captureId, MoneyAmount amount) 
            => _client.RefundMoney(paymentId, orderReference, captureId, amount);


        public Task<Result<StatusResponse>> RefreshStatus(string referenceCode)
        {
            return GetPayment()
                .Bind(GetStatus)
                .CheckIf(x => x.Item1.Status != x.Item2, SavePayment)
                .Map(MapToResponse);


            async Task<Result<Payment>> GetPayment()
            {
                var payment = await _context.Payments
                    .OrderByDescending(p => p.Created)
                    .FirstOrDefaultAsync(p => p.PaymentProcessor == PaymentProcessors.NGenius && p.ReferenceCode == referenceCode);

                return payment ?? Result.Failure<Payment>($"Payment for {referenceCode} not found");
            }


            async Task<Result<(Payment, PaymentStatuses)>> GetStatus(Payment payment)
            {
                var data = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
                var (_, isFailure, status) = await _client.GetStatus(data.InternalReferenceCode, payment.Currency);

                return isFailure 
                    ? Result.Failure<(Payment, PaymentStatuses)>("Status checking failed") 
                    : (payment, status);
            }


            async Task<Result> SavePayment((Payment Payment, PaymentStatuses Status) tuple)
            {
                var (payment, status) = tuple;
                payment.Status = status;
                payment.Modified = _dateTimeProvider.UtcNow();
                _context.Update(payment);
                await _context.SaveChangesAsync();
                await _bookingPaymentCallbackService.ProcessPaymentChanges(payment);
                return Result.Success();
            }


            StatusResponse MapToResponse((Payment Payment, PaymentStatuses Status) tuple) 
                => new (tuple.Payment.Status == PaymentStatuses.Authorized
                    ? CreditCardPaymentStatuses.Success
                    : CreditCardPaymentStatuses.Failed);
        }


        private async Task<Result<Payment>> CreatePayment(string ipAddress, MoneyAmount price, NGeniusPaymentResponse paymentResult)
        {
            var now = _dateTimeProvider.UtcNow();

            var info = new CreditCardPaymentInfo(customerIp: ipAddress, 
                externalId: paymentResult.PaymentId,
                message: string.Empty, 
                authorizationCode: string.Empty, 
                expirationDate: string.Empty,
                internalReferenceCode: paymentResult.OrderReference);

            var payment = new Payment
            {
                Amount = price.Amount,
                Currency = price.Currency,
                AccountNumber = string.Empty,
                Created = now,
                Modified = now,
                Status = PaymentStatuses.Created,
                Data = JsonConvert.SerializeObject(info),
                PaymentMethod = PaymentTypes.CreditCard,
                PaymentProcessor = PaymentProcessors.NGenius,
                ReferenceCode = paymentResult.MerchantOrderReference
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
        
        
        private async Task<Result<NGeniusPaymentResponse>> StorePaymentResults(string ipAddress, MoneyAmount price, 
            NGeniusPaymentResponse paymentResult) 
            => await CreatePayment(ipAddress, price, paymentResult)
                .Bind(p => _bookingPaymentCallbackService.ProcessPaymentChanges(p))
                .Map(() => paymentResult);


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly INGeniusClient _client;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
        private readonly IAgencyService _agencyService;
    }
}