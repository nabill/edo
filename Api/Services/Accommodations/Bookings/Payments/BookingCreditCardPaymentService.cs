using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Bookings.Invoices;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingCreditCardPaymentService : IBookingCreditCardPaymentService
    {
        public BookingCreditCardPaymentService(ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            ILogger<BookingCreditCardPaymentService> logger,
            IDateTimeProvider dateTimeProvider, IBookingDocumentsMailingService documentsMailingService,
            IBookingInfoService bookingInfoService, IBookingDocumentsService documentsService,
            IBookingPaymentCallbackService paymentCallbackService, IAgentContextService agentContextService,
            IJsonSerializer serializer, IInvoiceService invoiceService,
            IBookingRecordManager bookingRecordManager)
        {
            _bookingRecordManager = bookingRecordManager;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _documentsMailingService = documentsMailingService;
            _bookingInfoService = bookingInfoService;
            _documentsService = documentsService;
            _paymentCallbackService = paymentCallbackService;
            _agentContextService = agentContextService;
            _invoiceService = invoiceService;
            _serializer = serializer;
        }


        public async Task<Result<string>> Capture(Booking booking, ApiCaller apiCaller)
        {
            if (booking.PaymentType != PaymentTypes.CreditCard)
            {
                _logger.LogCaptureMoneyForBookingFailure(booking.ReferenceCode, booking.PaymentType);
                return Result.Failure<string>($"Invalid payment method: {booking.PaymentType}");
            }

            var result = await _creditCardPaymentProcessingService.CaptureMoney(booking.ReferenceCode, apiCaller, _paymentCallbackService);
            if (result.IsSuccess)
                _logger.LogCaptureMoneyForBookingSuccess(booking.ReferenceCode);

            return result;
        }


        public async Task<Result> Void(Booking booking, ApiCaller apiCaller)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                return Result.Failure($"Void is only available for payments with '{BookingPaymentStatuses.Authorized}' status");

            return await _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, apiCaller, _paymentCallbackService);
        }


        public async Task<Result> Refund(Booking booking, DateTimeOffset operationDate, ApiCaller apiCaller)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                return Result.Failure($"Refund is only available for payments with '{BookingPaymentStatuses.Captured}' status");

            return await _creditCardPaymentProcessingService.RefundMoney(booking.ReferenceCode, apiCaller, operationDate, _paymentCallbackService);
        }


        public async Task<Result> PayForAccountBooking(string referenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            return await GetBooking(referenceCode)
                .Tap(UpdateBookingInfo)
                .Ensure(IsBookingPaid, "Failed to pay for booking")
                .CheckIf(IsDeadlinePassed, CaptureMoney)
                .Tap(SendReceipt);


            Task<Result<Booking>> GetBooking(string code)
                => _bookingInfoService.GetAgentsBooking(code);


            bool IsBookingPaid(Booking booking)
                => booking.PaymentStatus == BookingPaymentStatuses.Authorized;


            bool IsDeadlinePassed(Booking booking)
                => booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();


            async Task<Result> CaptureMoney(Booking booking)
                => await Capture(booking, agent.ToApiCaller());


            async Task SendReceipt(Booking booking)
            {
                var (_, _, receiptInfo, _) = await _documentsService.GenerateReceipt(booking);

                await _documentsMailingService.SendReceiptToCustomer(receiptInfo, agent.Email);
            }


            async Task UpdateBookingInfo(Booking booking)
            {
                booking.TotalPrice = booking.CreditCardPrice;
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


        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly ILogger<BookingCreditCardPaymentService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingPaymentCallbackService _paymentCallbackService;
        private readonly IAgentContextService _agentContextService;
        private readonly IInvoiceService _invoiceService;
        private readonly IJsonSerializer _serializer;
        private readonly IBookingRecordManager _bookingRecordManager;
    }
}