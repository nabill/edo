using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings.Invoices;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusPaymentService : INGeniusPaymentService
    {
        public NGeniusPaymentService(IBookingRecordManager bookingRecordManager, IInvoiceService invoiceService,
            INGeniusClient client, IAgencyService agencyService, ICreditCardPaymentManagementService paymentService,
            IJsonSerializer serializer)
        {
            _bookingRecordManager = bookingRecordManager;
            _client = client;
            _agencyService = agencyService;
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _serializer = serializer;
        }


        public async Task<Result<NGeniusPaymentResponse>> Authorize(string referenceCode, string ipAddress, AgentContext agent)
        {
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<NGeniusPaymentResponse>(error);

            booking.TotalPrice = booking.CreditCardPrice;

            return await _agencyService.Get(agent)
                .Bind(CreateOrder)
                .Bind(StorePayment)
                .Tap(UpdateBookingInfo);


            Task<Result<NGeniusPaymentResponse>> CreateOrder(SlimAgencyInfo agency)
                => _client.CreateOrder(orderType: OrderTypes.Auth,
                    referenceCode: booking.ReferenceCode,
                    currency: booking.Currency,
                    price: booking.TotalPrice,
                    email: agent.Email,
                    billingAddress: (agent, agency).ToBillingAddress());


            async Task<Result<NGeniusPaymentResponse>> StorePayment(NGeniusPaymentResponse response)
            {
                var (_, isFailure, payment, error) = await _paymentService.Create(paymentId: response.PaymentId,
                    paymentOrderReference: response.OrderReference,
                    bookingReferenceCode: response.MerchantOrderReference,
                    price: booking.TotalPrice.ToMoneyAmount(booking.Currency),
                    ipAddress: ipAddress);

                return isFailure
                    ? Result.Failure<NGeniusPaymentResponse>(error)
                    : response;
            }


            async Task UpdateBookingInfo()
            {
                await _bookingRecordManager.Update(booking);

                var invoices = await _invoiceService.GetInvoices(ServiceTypes.HTL, ServiceSource.Internal, booking.ReferenceCode);

                if (invoices != default)
                {
                    foreach (var invoice in invoices)
                    {
                        var bookingInfo = _serializer.DeserializeObject<BookingInvoiceData>(invoice.Data!);
                        bookingInfo = new BookingInvoiceData(new MoneyAmount(booking.TotalPrice, booking.Currency), bookingInfo);

                        invoice.Data = _serializer.SerializeObject(bookingInfo);
                    }


                    await _invoiceService.Update(invoices);
                }
            }
        }


        public Task<Result<CreditCardCaptureResult>> Capture(string paymentId, string orderReference, MoneyAmount amount)
            => _client.CaptureMoney(paymentId, orderReference, amount);


        public Task<Result<CreditCardVoidResult>> Void(string paymentId, string orderReference, Currencies currency)
            => _client.VoidMoney(paymentId, orderReference, currency);


        public Task<Result<CreditCardRefundResult>> Refund(string paymentId, string orderReference, string captureId, MoneyAmount amount)
            => _client.RefundMoney(paymentId, orderReference, captureId, amount);


        public Task<Result<StatusResponse>> RefreshStatus(string referenceCode)
        {
            return _paymentService.Get(referenceCode)
                .Bind(GetStatus)
                .CheckIf(x => x.Item1.Status != x.Item2, SavePayment)
                .Map(MapToResponse);


            async Task<Result<(Payment, PaymentStatuses)>> GetStatus(Payment payment)
            {
                var data = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
                var (_, isFailure, status) = await _client.GetStatus(data.InternalReferenceCode, payment.Currency);

                return isFailure
                    ? Result.Failure<(Payment, PaymentStatuses)>("Status checking failed")
                    : (payment, status);
            }


            Task<Result> SavePayment((Payment Payment, PaymentStatuses Status) tuple)
            {
                var (payment, status) = tuple;
                return _paymentService.SetStatus(payment, status);
            }


            StatusResponse MapToResponse((Payment Payment, PaymentStatuses Status) tuple)
                => new(tuple.Payment.Status == PaymentStatuses.Authorized
                    ? CreditCardPaymentStatuses.Success
                    : CreditCardPaymentStatuses.Failed);
        }


        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly INGeniusClient _client;
        private readonly IAgencyService _agencyService;
        private readonly ICreditCardPaymentManagementService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IJsonSerializer _serializer;
    }
}