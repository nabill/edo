using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingPaymentService : IBookingPaymentService
    {
        public BookingPaymentService(IDateTimeProvider dateTimeProvider, 
            EdoContext context,
            ILogger<BookingPaymentService> logger,
            IAccountPaymentService accountPaymentService,
            ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            IPaymentNotificationService notificationService,
            IAgentService agentService,
            IBookingRecordsManager recordsManager)
        {
            _dateTimeProvider = dateTimeProvider;
            _context = context;
            _logger = logger;
            _accountPaymentService = accountPaymentService;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _notificationService = notificationService;
            _agentService = agentService;
            _recordsManager = recordsManager;
        }
        
        public async Task<Result<List<int>>> GetBookingsForCapture(DateTime deadlineDate)
        {
            if (deadlineDate == default)
                return Result.Failure<List<int>>("Deadline date should be specified");
            
            var date = deadlineDate.Date;

            var bookingIds = await (
                _context.Bookings
                    .Where(IsBookingValidForCapturePredicate)
                    .Where(b => b.CheckInDate <= date || (b.DeadlineDate.HasValue && b.DeadlineDate.Value.Date < date))
                    .Select(b => b.Id)
                )
                .ToListAsync();

            return Result.Ok(bookingIds);
        }


        public async Task<Result<ProcessResult>> CaptureMoneyForBookings(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            var bookings = await GetBookings();

            return await ValidateCount()
                .Map(ProcessBookings);


            Task<List<Booking>> GetBookings() => _context.Bookings
                .Where(booking => bookingIds.Contains(booking.Id))
                .Where(IsBookingValidForCapturePredicate)
                .ToListAsync();


            Result ValidateCount()
            {
                return bookings.Count != bookingIds.Count
                    ? Result.Failure("Invalid booking ids. Could not find some of requested bookings.")
                    : Result.Ok();
            }


            Task<ProcessResult> ProcessBookings()
            {
                return Combine(bookings.Select(ProcessBooking));


                Task<Result<string>> ProcessBooking(Booking booking)
                {
                    switch (booking.PaymentMethod)
                    {
                        case PaymentMethods.BankTransfer:
                            return _accountPaymentService.CaptureMoney(booking, serviceAccount.ToUserInfo());
                        case PaymentMethods.CreditCard:
                            return _creditCardPaymentProcessingService.CaptureMoney(booking.ReferenceCode, serviceAccount.ToUserInfo(), this);
                        default: return Task.FromResult(Result.Failure<string>($"Invalid payment method: {booking.PaymentMethod}"));
                    }
                }
            }
        }


        public Task<Result> VoidMoney(Booking booking, UserInfo user)
        {
            // TODO: Add logging
            // TODO: Implement refund money if status is paid with deadline penalty
            if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                return Task.FromResult(Result.Ok());

            switch (booking.PaymentMethod)
            {
                case PaymentMethods.BankTransfer:
                    return _accountPaymentService.VoidMoney(booking, user);
                case PaymentMethods.CreditCard:
                    return _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, user, this);
                default: return Task.FromResult(Result.Failure($"Could not void money for the booking with a payment method '{booking.PaymentMethod}'"));
            }
        }


        public async Task<Result> CompleteOffline(int bookingId, Administrator administratorContext)
        {
            // TODO: Add admin actions audit log NIJO-659
            return await GetBooking()
                .Bind(CheckBookingCanBeCompleted)
                .Tap(Complete)
                .Tap(SendInvoiceToAgent);


            async Task<Result<Booking>> GetBooking()
            {
                var (_, isFailure, booking, _) = await _recordsManager.Get(bookingId);
                return isFailure
                    ? Result.Failure<Booking>($"Could not find booking with id {bookingId}")
                    : Result.Ok(booking);
            }


            Result<Booking> CheckBookingCanBeCompleted(Booking booking)
                => booking.PaymentStatus == BookingPaymentStatuses.NotPaid
                    ? Result.Ok(booking)
                    : Result.Failure<Booking>($"Could not complete booking. Invalid payment status: {booking.PaymentStatus}");


            Task Complete(Booking booking)
            {
                booking.PaymentMethod = PaymentMethods.Offline;
                return ChangeBookingPaymentStatusToCaptured(booking);
            }


            async Task SendInvoiceToAgent(Booking booking)
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(c => c.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Send bill after offline payment: could not find agent with id '{0}' for the booking '{1}'", booking.AgentId,
                        booking.ReferenceCode);
                    return;
                }

                await _notificationService.SendInvoiceToCustomer(new PaymentInvoice(agent.Email,
                    booking.TotalPrice,
                    booking.Currency,
                    _dateTimeProvider.UtcNow(),
                    PaymentMethods.Offline,
                    booking.ReferenceCode,
                    $"{agent.LastName} {agent.FirstName}"));
            }
        }


        public async Task<Result<ProcessResult>> NotifyPaymentsNeeded(List<int> bookingIds)
        {
            var bookings = await GetBookings();

            return await Validate()
                .Map(ProcessBookings);

            Task<List<Booking>> GetBookings() => _context.Bookings.Where(booking => bookingIds.Contains(booking.Id)).ToListAsync();


            Result Validate()
            {
                return bookings.Count != bookingIds.Count
                    ? Result.Failure("Invalid booking ids. Could not find some of requested bookings.")
                    : Result.Combine(bookings.Select(ValidateBooking).ToArray());


                Result ValidateBooking(Booking booking)
                    => GenericValidator<Booking>.Validate(v =>
                    {
                        v.RuleFor(c => c.PaymentStatus)
                            .Must(status => booking.PaymentStatus == BookingPaymentStatuses.NotPaid)
                            .WithMessage(
                                $"Invalid payment status for the booking '{booking.ReferenceCode}': {booking.PaymentStatus}");
                        v.RuleFor(c => c.Status)
                            .Must(status => BookingStatusesForPayment.Contains(status))
                            .WithMessage($"Invalid booking status for the booking '{booking.ReferenceCode}': {booking.Status}");
                        v.RuleFor(c => c.PaymentMethod)
                            .Must(method => method == PaymentMethods.BankTransfer ||
                                method == PaymentMethods.CreditCard)
                            .WithMessage($"Invalid payment method for the booking '{booking.ReferenceCode}': {booking.PaymentMethod}");
                    }, booking);
            }


            Task<ProcessResult> ProcessBookings()
            {
                return Combine(bookings.Select(ProcessBooking));


                Task<Result<string>> ProcessBooking(Booking booking)
                {
                    return Notify()
                        .Finally(CreateResult);


                    async Task<Result> Notify()
                    {
                        var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                        if (agent == default)
                            return Result.Failure($"Could not find agent with id {booking.AgentId}");

                        return await _notificationService.SendNeedPaymentNotificationToCustomer(new PaymentInvoice(agent.Email,
                            booking.TotalPrice,
                            booking.Currency,
                            DateTime.MinValue,
                            booking.PaymentMethod,
                            booking.ReferenceCode,
                            $"{agent.LastName} {agent.FirstName}"));
                    }


                    Result<string> CreateResult(Result result)
                        => result.IsSuccess
                            ? Result.Ok($"Payment for the booking '{booking.ReferenceCode}' completed.")
                            : Result.Failure<string>($"Unable to complete payment for the booking '{booking.ReferenceCode}'. Reason: {result.Error}");
                }
            }
        }


        private async Task<ProcessResult> Combine(IEnumerable<Task<Result<string>>> results)
        {
            var builder = new StringBuilder();

            foreach (var result in results)
            {
                var (_, isFailure, value, error) = await result;
                builder.AppendLine(isFailure ? error : value);
            }

            return new ProcessResult(builder.ToString());
        }


        private Task ChangeBookingPaymentStatusToCaptured(Booking booking)
        {
            booking.PaymentStatus = BookingPaymentStatuses.Captured;
            _context.Bookings.Update(booking);
            return _context.SaveChangesAsync();
        }
        
        private static readonly HashSet<BookingStatusCodes> BookingStatusesForPayment = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.Pending, BookingStatusCodes.Confirmed
        };
        
        public async Task<Result<MoneyAmount>> GetServicePrice(string referenceCode)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == referenceCode);
            if(booking == default)
                return Result.Failure<MoneyAmount>("Could not find booking");

            return Result.Ok(new MoneyAmount(booking.TotalPrice, booking.Currency));
        }


        public async Task<Result> ProcessPaymentChanges(Payment payment)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == payment.ReferenceCode);
            if(booking == default)
                return Result.Failure($"Could not find booking for payment '{payment.ReferenceCode}'");
            
            switch (payment.Status)
            {
                case PaymentStatuses.Authorized:
                    booking.PaymentStatus = BookingPaymentStatuses.Authorized;
                    break;
                case PaymentStatuses.Captured:
                    booking.PaymentStatus = BookingPaymentStatuses.Captured;
                    break;
                case PaymentStatuses.Voided:
                    booking.PaymentStatus = BookingPaymentStatuses.Voided;
                    break;
                case PaymentStatuses.Refunded:
                    booking.PaymentStatus = BookingPaymentStatuses.Refunded;
                    break;
                
                default: return Result.Ok();
            }

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            
            _context.Entry(booking).State = EntityState.Detached;
            
            return Result.Ok();
        }


        public async Task<Result<AgentInfoInAgency>> GetServiceBuyer(string referenceCode)
        {
            var (_, isFailure, booking, error) = await _recordsManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<AgentInfoInAgency>(error);

            return await _agentService.GetAgent(booking.AgencyId, booking.AgentId);
        }


        private static readonly Expression<Func<Booking, bool>> IsBookingValidForCapturePredicate = booking => BookingStatusesForPayment.Contains(booking.Status) &&
            booking.PaymentStatus == BookingPaymentStatuses.Authorized &&
            (booking.PaymentMethod == PaymentMethods.BankTransfer || booking.PaymentMethod == PaymentMethods.CreditCard);

        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BookingPaymentService> _logger;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IPaymentNotificationService _notificationService;
        private readonly IAgentService _agentService;
        private readonly IBookingRecordsManager _recordsManager;
    }
}