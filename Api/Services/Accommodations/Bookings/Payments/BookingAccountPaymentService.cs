using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingAccountPaymentService : IBookingAccountPaymentService
    {
        public BookingAccountPaymentService(IPaymentNotificationService notificationService,
            IAccountPaymentService accountPaymentService,
            IBookingDocumentsService documentsService,
            ILogger<BookingAccountPaymentService> logger,
            EdoContext context)
        {
            _notificationService = notificationService;
            _accountPaymentService = accountPaymentService;
            _documentsService = documentsService;
            _logger = logger;
            _context = context;
        }


        public async Task<Result<string>> Charge(Booking booking, UserInfo user)
        {
            return await CheckPaymentMethod()
                .Bind(Charge)
                .Bind(SendReceipt)
                .Finally(WriteLog);


            Result CheckPaymentMethod()
                => booking.PaymentMethod == PaymentMethods.BankTransfer
                    ? Result.Success()
                    : Result.Failure($"Failed to charge money for a booking with reference code: '{booking.ReferenceCode}'. " +
                        $"Error: Invalid payment method: {booking.PaymentMethod}");


            async Task<Result<string>> Charge()
            {
                var (_, isFailure, _, error) = await _accountPaymentService.Charge(booking, user, booking.AgencyId, null);
                if (isFailure)
                    return Result.Failure<string>($"Unable to charge payment for a booking with reference code: '{booking.ReferenceCode}'. " +
                        $"Error while charging: {error}");

                return Result.Success($"Successfully charged money for a booking with reference code: '{booking.ReferenceCode}'");
            }


            async Task<Result<string>> SendReceipt(string chargeMessage)
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                var (_, isFailure, receiptInfo, error) = await _documentsService.GenerateReceipt(booking.Id, booking.AgentId);

                if (isFailure)
                    return Result.Failure<string>($"Unable to charge payment for a booking with reference code: '{booking.ReferenceCode}'. " +
                        $"Error while sending receipt: {error}");

                await _notificationService.SendReceiptToCustomer(receiptInfo, agent.Email);
                return chargeMessage;
            }


            Result<string> WriteLog(Result<string> result)
            {
                if (result.IsSuccess)
                    _logger.LogChargeMoneyForBookingSuccess(result.Value);
                else
                    _logger.LogChargeMoneyForBookingFailure(result.Error);

                return result;
            }
        }


        public async Task<Result> Refund(Booking booking, UserInfo user)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                return Result.Failure($"Cannot refund money for status {booking.PaymentStatus}");

            return await _accountPaymentService.Refund(booking, user);
        }


        private readonly IPaymentNotificationService _notificationService;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly IBookingDocumentsService _documentsService;
        private readonly ILogger<BookingAccountPaymentService> _logger;
        private readonly EdoContext _context;
    }
}