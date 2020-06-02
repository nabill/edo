using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingsProcessingService : IBookingsProcessingService
    {
        public BookingsProcessingService(IBookingPaymentService bookingPaymentService,
            IPaymentNotificationService notificationService,
            IBookingService bookingService,
            IDateTimeProvider dateTimeProvider,
            EdoContext context)
        {
            _bookingPaymentService = bookingPaymentService;
            _notificationService = notificationService;
            _bookingService = bookingService;
            _dateTimeProvider = dateTimeProvider;
            _context = context;
        }


        public Task<List<int>> GetForCapture(DateTime date)
        {
            date = date.Date;
            return _context.Bookings
                .Where(IsBookingValidForCapturePredicate)
                .Where(b => b.CheckInDate <= date || (b.DeadlineDate.HasValue && b.DeadlineDate.Value.Date <= date))
                .Select(b => b.Id)
                .ToListAsync();
        }


        public Task<Result<ProcessResult>> Capture(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForCapturePredicate,
                Capture,
                serviceAccount);

            Task<Result<string>> Capture(Booking booking, UserInfo serviceAcc) => _bookingPaymentService.CaptureMoney(booking, serviceAccount.ToUserInfo());
        }


        public Task<List<int>> GetForNotification(DateTime date) => GetForCapture(date.AddDays(DaysBeforeNotification));


        public Task<Result<ProcessResult>> NotifyDeadlineApproaching(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForCapturePredicate,
                Notify,
                serviceAccount);


            Task<Result<string>> Notify(Booking booking, UserInfo _)
            {
                return Notify()
                    .Finally(CreateResult);


                async Task<Result> Notify()
                {
                    var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                    if (agent == default)
                        return Result.Failure($"Could not find agent with id {booking.AgentId}");

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
                        : Result.Failure<string>($"Unable to complete payment for the booking '{booking.ReferenceCode}'. Reason: {result.Error}");
            }
        }


        public Task<List<int>> GetForCancellation()
        {
            return _context.Bookings
                .Where(IsBookingValidForCancelPredicate)
                .Select(booking => booking.Id)
                .ToListAsync();
        }


        public Task<Result<ProcessResult>> Cancel(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForCancelPredicate,
                ProcessBooking,
                serviceAccount);


            Task<Result<string>> ProcessBooking(Booking booking, UserInfo _)
            {
                return _bookingService.Cancel(booking.Id, serviceAccount)
                    .Finally(CreateResult);


                Result<string> CreateResult(Result<VoidObject, ProblemDetails> result)
                    => result.IsSuccess
                        ? Result.Ok($"Booking '{booking.ReferenceCode}' was cancelled.")
                        : Result.Failure<string>($"Unable to cancel booking '{booking.ReferenceCode}'. Reason: {result.Error.Detail}");
            }
        }


        private async Task<Result<ProcessResult>> ExecuteBatchAction(List<int> bookingIds,
            Expression<Func<Booking, bool>> predicate,
            Func<Booking, UserInfo, Task<Result<string>>> action,
            ServiceAccount serviceAccount)
        {
            var bookings = await GetBookings();

            return await ValidateCount()
                .Map(ProcessBookings);


            Task<List<Booking>> GetBookings()
                => _context.Bookings
                    .Where(booking => bookingIds.Contains(booking.Id))
                    .Where(predicate)
                    .ToListAsync();


            Result ValidateCount()
                => bookings.Count != bookingIds.Count
                    ? Result.Failure("Invalid booking ids. Could not find some of requested bookings.")
                    : Result.Ok();


            Task<ProcessResult> ProcessBookings() => Combine(bookings.Select(booking => action(booking, serviceAccount.ToUserInfo())));


            async Task<ProcessResult> Combine(IEnumerable<Task<Result<string>>> results)
            {
                var builder = new StringBuilder();

                foreach (var result in results)
                {
                    var (_, isFailure, value, error) = await result;
                    builder.AppendLine(isFailure ? error : value);
                }

                return new ProcessResult(builder.ToString());
            }
        }


        private const int DaysBeforeNotification = 3;


        private static readonly Expression<Func<Booking, bool>> IsBookingValidForCancelPredicate = booking
            => BookingStatusesForCancellation.Contains(booking.Status) && 
            PaymentStatusesForCancellation.Contains(booking.PaymentStatus);
        
        private static readonly HashSet<BookingStatusCodes> BookingStatusesForCancellation = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.Pending, BookingStatusCodes.Confirmed
        };

        private static readonly HashSet<BookingPaymentStatuses> PaymentStatusesForCancellation = new HashSet<BookingPaymentStatuses>
        {
            BookingPaymentStatuses.NotPaid, BookingPaymentStatuses.Refunded, BookingPaymentStatuses.Voided
        };

        private static readonly Expression<Func<Booking, bool>> IsBookingValidForCapturePredicate = booking
            => BookingStatusesForPayment.Contains(booking.Status) &&
            PaymentMethodsForCapture.Contains(booking.PaymentMethod) &&
            booking.PaymentStatus == BookingPaymentStatuses.Authorized;

        private static readonly HashSet<BookingStatusCodes> BookingStatusesForPayment = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.Pending, BookingStatusCodes.Confirmed, BookingStatusCodes.InternalProcessing, BookingStatusCodes.WaitingForResponse
        };
        
        private static readonly HashSet<PaymentMethods> PaymentMethodsForCapture = new HashSet<PaymentMethods>
        {
            PaymentMethods.BankTransfer, PaymentMethods.CreditCard
        };


        private readonly IBookingPaymentService _bookingPaymentService;
        private readonly IBookingService _bookingService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPaymentNotificationService _notificationService;
    }
}