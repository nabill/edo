using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingPaymentService : IBookingPaymentService
    {
        public BookingPaymentService(IDateTimeProvider dateTimeProvider, 
            IAdministratorContext adminContext, 
            EdoContext context,
            ILogger<BookingPaymentService> logger,
            IAccountPaymentService accountPaymentService,
            ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            IPaymentNotificationService notificationService,
            IServiceAccountContext serviceAccountContext)
        {
            _dateTimeProvider = dateTimeProvider;
            _adminContext = adminContext;
            _context = context;
            _logger = logger;
            _accountPaymentService = accountPaymentService;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _notificationService = notificationService;
            _serviceAccountContext = serviceAccountContext;
        }
        
        public async Task<Result<List<int>>> GetBookingsForCapture(DateTime deadlineDate)
        {
            if (deadlineDate == default)
                return Result.Fail<List<int>>("Deadline date should be specified");

            var (_, isFailure, _, error) = await _serviceAccountContext.GetUserInfo();
            if (isFailure)
                return Result.Fail<List<int>>(error);

            var bookings = await _context.Bookings
                .Where(booking =>
                    BookingStatusesForPayment.Contains(booking.Status) && booking.PaymentStatus == BookingPaymentStatuses.Authorized &&
                    (booking.PaymentMethod == PaymentMethods.BankTransfer || booking.PaymentMethod == PaymentMethods.CreditCard))
                .ToListAsync();

            var date = deadlineDate.Date;
            var bookingIds = bookings
                .Where(IsTimeToCaptureMoney)
                .Select(booking => booking.Id)
                .ToList();

            return Result.Ok(bookingIds);


            bool IsTimeToCaptureMoney(Booking booking)
            {
                if (booking.CheckInDate <= date)
                    return true;
                
                return booking.DeadlineDate != null && booking.DeadlineDate.Value.Date < date;
            }
        }


        public async Task<Result<ProcessResult>> CaptureMoneyForBookings(List<int> bookingIds)
        {
            var (_, isUserFailure, _, userError) = await _serviceAccountContext.GetUserInfo();
            if (isUserFailure)
                return Result.Fail<ProcessResult>(userError);

            var bookings = await GetBookings();

            return await Validate()
                .OnSuccess(ProcessBookings);


            Task<List<Booking>> GetBookings()
            {
                var ids = bookingIds;
                return _context.Bookings.Where(booking => ids.Contains(booking.Id)).ToListAsync();
            }


            Result Validate()
            {
                return bookings.Count != bookingIds.Count
                    ? Result.Fail("Invalid booking ids. Could not find some of requested bookings.")
                    : Result.Combine(bookings.Select(ValidateBooking).ToArray());


                Result ValidateBooking(Booking booking)
                    => GenericValidator<Booking>.Validate(v =>
                    {
                        v.RuleFor(c => c.PaymentStatus)
                            .Must(status => booking.PaymentStatus == BookingPaymentStatuses.Authorized)
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
                    switch (booking.PaymentMethod)
                    {
                        case PaymentMethods.BankTransfer:
                            return _accountPaymentService.CaptureMoney(booking);
                        case PaymentMethods.CreditCard:
                            return _creditCardPaymentProcessingService.CaptureMoney(booking.ReferenceCode, this);
                        default: return Task.FromResult(Result.Fail<string>($"Invalid payment method: {booking.PaymentMethod}"));
                    }
                }
            }
        }


        public Task<Result> VoidMoney(Booking booking)
        {
            // TODO: Add logging
            // TODO: Implement refund money if status is paid with deadline penalty
            if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                return Task.FromResult(Result.Ok());

            switch (booking.PaymentMethod)
            {
                case PaymentMethods.BankTransfer:
                    return _accountPaymentService.VoidMoney(booking);
                case PaymentMethods.CreditCard:
                    return _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, this);
                default: return Task.FromResult(Result.Fail($"Could not void money for the booking with a payment method '{booking.PaymentMethod}'"));
            }
        }


        public async Task<Result> CompleteOffline(int bookingId)
        {
            return await _adminContext.GetCurrent()
                .OnSuccess(GetBooking)
                .OnSuccess(CheckBookingCanBeCompleted)
                .OnSuccess(Complete)
                .OnSuccess(SendBillToAgent);


            async Task<Result<Booking>> GetBooking()
            {
                var entity = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
                return entity == null
                    ? Result.Fail<Booking>($"Could not find booking with id {bookingId}")
                    : Result.Ok(entity);
            }


            Result<Booking> CheckBookingCanBeCompleted(Booking booking)
                => booking.PaymentStatus == BookingPaymentStatuses.NotPaid
                    ? Result.Ok(booking)
                    : Result.Fail<Booking>($"Could not complete booking. Invalid payment status: {booking.PaymentStatus}");


            Task Complete(Booking booking)
            {
                booking.PaymentMethod = PaymentMethods.Offline;
                return ChangeBookingPaymentStatusToCaptured(booking);
            }


            async Task SendBillToAgent(Booking booking)
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(c => c.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Send bill after offline payment: could not find agent with id '{0}' for the booking '{1}'", booking.AgentId,
                        booking.ReferenceCode);
                    return;
                }

                await _notificationService.SendBillToCustomer(new PaymentBill(agent.Email,
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
            var (_, isUserFailure, _, userError) = await _serviceAccountContext.GetUserInfo();
            if (isUserFailure)
                return Result.Fail<ProcessResult>(userError);

            var bookings = await GetBookings();

            return await Validate()
                .OnSuccess(ProcessBookings);

            Task<List<Booking>> GetBookings() => _context.Bookings.Where(booking => bookingIds.Contains(booking.Id)).ToListAsync();


            Result Validate()
            {
                return bookings.Count != bookingIds.Count
                    ? Result.Fail("Invalid booking ids. Could not find some of requested bookings.")
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
                        .OnBoth(CreateResult);


                    async Task<Result> Notify()
                    {
                        var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                        if (agent == default)
                            return Result.Fail($"Could not find agent with id {booking.AgentId}");

                        return await _notificationService.SendNeedPaymentNotificationToCustomer(new PaymentBill(agent.Email,
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
                            : Result.Fail<string>($"Unable to complete payment for the booking '{booking.ReferenceCode}'. Reason: {result.Error}");
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
                return Result.Fail<MoneyAmount>("Could not find booking");

            return Result.Ok(new MoneyAmount(booking.TotalPrice, booking.Currency));
        }


        public async Task<Result> ProcessPaymentChanges(Payment payment)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == payment.ReferenceCode);
            if(booking == default)
                return Result.Fail($"Could not find booking for payment '{payment.ReferenceCode}'");
            
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

        private readonly IAdministratorContext _adminContext;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BookingPaymentService> _logger;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IPaymentNotificationService _notificationService;
        private readonly IServiceAccountContext _serviceAccountContext;
    }
}