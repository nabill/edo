using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Api.Extensions;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BatchProcessing
{
    public class BookingsProcessingService : IBookingsProcessingService
    {
        public BookingsProcessingService(IBookingAccountPaymentService accountPaymentService,
            IBookingCreditCardPaymentService creditCardPaymentService,
            ISupplierBookingManagementService supplierBookingManagementService,
            IBookingNotificationService bookingNotificationService,
            IBookingReportsService reportsService,
            EdoContext context,
            IBookingRecordsUpdater bookingRecordsUpdater,
            IDateTimeProvider dateTimeProvider)
        {
            _accountPaymentService = accountPaymentService;
            _creditCardPaymentService = creditCardPaymentService;
            _supplierBookingManagementService = supplierBookingManagementService;
            _bookingNotificationService = bookingNotificationService;
            _reportsService = reportsService;
            _context = context;
            _bookingRecordsUpdater = bookingRecordsUpdater;
            _dateTimeProvider = dateTimeProvider;
        }


        public Task<List<int>> GetForCapture(DateTimeOffset date)
        {
            return _context.Bookings
                .Where(IsBookingValidForCapturePredicate)
                .Where(b => b.CheckInDate <= date
                    || (b.DeadlineDate.HasValue && b.DeadlineDate.Value <= date))
                .Select(b => b.Id)
                .ToListAsync();
        }


        public Task<Result<BatchOperationResult>> Capture(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForCapturePredicate,
                Capture,
                serviceAccount);

            Task<Result<string>> Capture(Booking booking, ApiCaller apiCaller)
                => _creditCardPaymentService.Capture(booking, serviceAccount.ToApiCaller());
        }


        public Task<List<int>> GetForCharge(DateTimeOffset date)
        {
            return _context.Bookings
                .Where(IsBookingValidForChargePredicate)
                .Where(b => b.DeadlineDate.HasValue && b.DeadlineDate.Value <= date)
                .Select(b => b.Id)
                .ToListAsync();
        }


        public Task<Result<BatchOperationResult>> Charge(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForChargePredicate,
                Charge,
                serviceAccount);


            async Task<Result<string>> Charge(Booking booking, ApiCaller apiCaller)
            {
                if (BookingStatusesNeededRefreshBeforePayment.Contains(booking.Status))
                {
                    var (_, isRefreshingFailure, refreshingError) = await _supplierBookingManagementService.RefreshStatus(booking, apiCaller,
                        BookingChangeEvents.Charge);

                    if (isRefreshingFailure)
                    {
                        await _bookingRecordsUpdater.ChangeStatus(booking, BookingStatuses.ManualCorrectionNeeded, _dateTimeProvider.UtcNow(), apiCaller, new BookingChangeReason
                        {
                            Source = BookingChangeSources.System,
                            Event = BookingChangeEvents.Charge,
                            Reason = "Failure in refreshing booking status before payment"
                        });
                        return Result.Failure<string>(refreshingError);
                    }

                    // Need to get fresh information about the booking
                    booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == booking.ReferenceCode);

                    if (BookingStatusesNeededRefreshBeforePayment.Contains(booking.Status))
                    {
                        await _bookingRecordsUpdater.ChangeStatus(booking, BookingStatuses.ManualCorrectionNeeded, _dateTimeProvider.UtcNow(), apiCaller, new BookingChangeReason
                        {
                            Source = BookingChangeSources.System,
                            Event = BookingChangeEvents.Charge,
                            Reason = "After refreshing the booking received a status requiring refreshing"
                        });
                        return Result.Failure<string>($"Booking {booking.ReferenceCode} with status {booking.Status} cannot be charged");
                    }
                }

                var chargeResult = await _accountPaymentService.Charge(booking, apiCaller);

                if (chargeResult.IsFailure)
                {
                    await _bookingRecordsUpdater.ChangeStatus(booking, BookingStatuses.ManualCorrectionNeeded, _dateTimeProvider.UtcNow(), apiCaller, new BookingChangeReason
                    {
                        Source = BookingChangeSources.System,
                        Event = BookingChangeEvents.Charge,
                        Reason = "It is impossible charge money from a virtual account"
                    });
                }

                return chargeResult;
            }
        }


        public Task<List<int>> GetForNotification(DateTimeOffset date)
        {
            date = date.AddDays(DaysBeforeNotification);
            return _context.Bookings
                .Where(IsBookingValidForDeadlineNotification)
                .Where(b => b.CheckInDate == date || (b.DeadlineDate.HasValue && b.DeadlineDate.Value == date))
                .Select(b => b.Id)
                .ToListAsync();
        }


        public Task<Result<BatchOperationResult>> NotifyDeadlineApproaching(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForDeadlineNotification,
                Notify,
                serviceAccount);


            Task<Result<string>> Notify(Booking booking, ApiCaller _)
            {
                return NotifyAgent()
                    .Finally(CreateResult);


                async Task<Result> NotifyAgent()
                {
                    var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                    if (agent == default)
                        return Result.Failure($"Could not find agent with id {booking.AgentId}");

                    return await _bookingNotificationService.NotifyDeadlineApproaching(booking.Id, agent.Email);
                }


                Result<string> CreateResult(Result result)
                    => result.IsSuccess
                        ? Result.Success($"Notification for the booking '{booking.ReferenceCode}' was sent.")
                        : Result.Failure<string>($"Unable to notify agent for the booking '{booking.ReferenceCode}'. Reason: {result.Error}");
            }
        }


        public async Task<Result<BatchOperationResult>> NotifyOfflineDeadlineApproaching()
        {
            return await GetOfflineBookings()
                .Bind(Notify);


            async Task<Result<Dictionary<Booking, OfflineDeadlineNotifications>>> GetOfflineBookings()
            {
                var results = new Dictionary<Booking, OfflineDeadlineNotifications>();

                foreach (var item in (OfflineDeadlineNotifications[])Enum.GetValues(typeof(OfflineDeadlineNotifications)))
                {
                    var bookings = item switch
                    {
                        OfflineDeadlineNotifications.FifteenDays
                            => await GetOfflineBookingsByDays(15, OfflineDeadlineNotifications.FifteenDays,
                                OfflineDeadlineNotifications.AfterBookingConfirmed),

                        OfflineDeadlineNotifications.SevenDays
                            => await GetOfflineBookingsByDays(7, OfflineDeadlineNotifications.SevenDays,
                                FifteenDaysBeforeSent),

                        OfflineDeadlineNotifications.ThreeDays
                            => await GetOfflineBookingsByDays(3, OfflineDeadlineNotifications.ThreeDays,
                                SevenDaysBeforeSent),

                        OfflineDeadlineNotifications.TwoDays
                            => await GetOfflineBookingsByDays(2, OfflineDeadlineNotifications.TwoDays,
                                ThreeDaysBeforeSent),

                        OfflineDeadlineNotifications.OneDay
                            => await GetOfflineBookingsByDays(1, OfflineDeadlineNotifications.OneDay,
                                TwoDaysBeforeSent),

                        _ => null
                    };

                    if (bookings is not null)
                        results = results.Union(bookings).ToDictionary(b => b.Key, b => b.Value);
                }

                return results;
            }


            async Task<Result<BatchOperationResult>> Notify(Dictionary<Booking, OfflineDeadlineNotifications> bookings)
                => await Combine(bookings.Select(async pair
                    => await _bookingNotificationService.NotifyOfflineDeadlineApproaching(pair.Key.Id, pair.Value)));


            async Task<BatchOperationResult> Combine(IEnumerable<Task<Result>> actions)
            {
                var builder = new StringBuilder();
                bool hasErrors = false;

                foreach (var action in actions)
                {
                    var result = await action;
                    if (result.IsFailure)
                        hasErrors = true;

                    builder.AppendLine(result.IsFailure ? result.Error : string.Empty);
                }

                return new BatchOperationResult(builder.ToString(), hasErrors);
            }
        }


        public Task<List<int>> GetForCancellation(DateTimeOffset date)
        {
            return _context.Bookings
                .Where(IsBookingValidForCancelPredicate)
                .Where(b => b.DeadlineDate.HasValue && b.DeadlineDate.Value <= date)
                .Select(booking => booking.Id)
                .ToListAsync();
        }


        public Task<Result<BatchOperationResult>> Cancel(List<int> bookingIds, ServiceAccount serviceAccount)
        {
            return ExecuteBatchAction(bookingIds,
                IsBookingValidForCancelPredicate,
                ProcessBooking,
                serviceAccount);


            Task<Result<string>> ProcessBooking(Booking booking, ApiCaller _)
            {
                return _supplierBookingManagementService.Cancel(booking, serviceAccount.ToApiCaller(), BookingChangeEvents.Cancel)
                    .Finally(CreateResult);


                Result<string> CreateResult(Result result)
                    => result.IsSuccess
                        ? Result.Success($"Booking '{booking.ReferenceCode}' was cancelled.")
                        : Result.Failure<string>($"Unable to cancel booking '{booking.ReferenceCode}'. Reason: {result.Error}");
            }
        }


        public async Task<BatchOperationResult> SendBookingSummaryReports()
        {
            var rolesWithPermission = await _context.AgentRoles
                .Where(r => r.Permissions.HasFlag(InAgencyPermissions.ReceiveBookingSummary))
                .Select(x => x.Id)
                .ToListAsync();

            var agencyIds = await _context.Agencies
                .Where(a => a.IsActive)
                .Where(a => _context.AgentAgencyRelations.Any(r => r.AgencyId == a.Id && r.IsActive
                    && r.AgentRoleIds.Any(rr => rolesWithPermission.Contains(rr))))
                .Where(a => _context.AgencyAccounts.Any(acc => acc.AgencyId == a.Id && acc.Currency == Currencies.USD))
                .Select(agency => agency.Id)
                .ToListAsync();

            var builder = new StringBuilder();
            var hasErrors = false;

            foreach (var agencyId in agencyIds)
            {
                var (_, isFailure, _, error) = await _reportsService.SendBookingReports(agencyId);
                if (isFailure)
                {
                    hasErrors = true;
                    builder.AppendLine(error);
                }
            }

            return new BatchOperationResult(builder.ToString(), hasErrors);
        }


        private async Task<Dictionary<Booking, OfflineDeadlineNotifications>> GetOfflineBookingsByDays(int days,
            OfflineDeadlineNotifications currentNotification, OfflineDeadlineNotifications notificationsAlreadySent)
        {
            var bookings = await _context.Bookings
                .Where(b => b.DeadlineDate.HasValue && (b.DeadlineDate!.Value - _dateTimeProvider.UtcNow()).Days <= days
                    && b.OfflineDeadlineNotificationsSent != null)
                .ToListAsync();

            return bookings
                .Where(b => b.OfflineDeadlineNotificationsSent.Has(notificationsAlreadySent)
                    && !b.OfflineDeadlineNotificationsSent.Has(currentNotification))
                .ToDictionary(b => b, b => currentNotification);
        }


        private async Task<Result<BatchOperationResult>> ExecuteBatchAction(List<int> bookingIds,
            Expression<Func<Booking, bool>> predicate,
            Func<Booking, ApiCaller, Task<Result<string>>> action,
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
                    : Result.Success();


            Task<BatchOperationResult> ProcessBookings() => Combine(bookings.Select(booking => action(booking, serviceAccount.ToApiCaller())));


            async Task<BatchOperationResult> Combine(IEnumerable<Task<Result<string>>> results)
            {
                var builder = new StringBuilder();
                bool hasErrors = false;

                foreach (var result in results)
                {
                    var (_, isFailure, value, error) = await result;
                    if (isFailure)
                        hasErrors = true;

                    builder.AppendLine(isFailure ? error : value);
                }

                return new BatchOperationResult(builder.ToString(), hasErrors);
            }
        }


        private const int DaysBeforeNotification = 3;

        private const OfflineDeadlineNotifications FifteenDaysBeforeSent = OfflineDeadlineNotifications.AfterBookingConfirmed
            | OfflineDeadlineNotifications.FifteenDays;

        private const OfflineDeadlineNotifications SevenDaysBeforeSent = OfflineDeadlineNotifications.AfterBookingConfirmed
            | OfflineDeadlineNotifications.FifteenDays | OfflineDeadlineNotifications.SevenDays;

        private const OfflineDeadlineNotifications ThreeDaysBeforeSent = OfflineDeadlineNotifications.AfterBookingConfirmed
            | OfflineDeadlineNotifications.FifteenDays | OfflineDeadlineNotifications.SevenDays
            | OfflineDeadlineNotifications.ThreeDays;

        private const OfflineDeadlineNotifications TwoDaysBeforeSent = OfflineDeadlineNotifications.AfterBookingConfirmed
            | OfflineDeadlineNotifications.FifteenDays | OfflineDeadlineNotifications.SevenDays
            | OfflineDeadlineNotifications.ThreeDays | OfflineDeadlineNotifications.TwoDays;

        private const OfflineDeadlineNotifications OneDayBeforeSent = OfflineDeadlineNotifications.AfterBookingConfirmed
            | OfflineDeadlineNotifications.FifteenDays | OfflineDeadlineNotifications.SevenDays
            | OfflineDeadlineNotifications.ThreeDays | OfflineDeadlineNotifications.TwoDays
            | OfflineDeadlineNotifications.OneDay;


        private static readonly Expression<Func<Booking, bool>> IsBookingValidForCancelPredicate = booking
            => BookingStatusesForCancellation.Contains(booking.Status) &&
            PaymentStatusesForCancellation.Contains(booking.PaymentStatus) &&
            PaymentMethodsForCancellation.Contains(booking.PaymentType);

        private static readonly HashSet<BookingStatuses> BookingStatusesForCancellation = new HashSet<BookingStatuses>
        {
            BookingStatuses.Confirmed
        };

        private static readonly HashSet<BookingPaymentStatuses> PaymentStatusesForCancellation = new()
        {
            BookingPaymentStatuses.NotPaid, BookingPaymentStatuses.Refunded, BookingPaymentStatuses.Voided
        };

        private static readonly HashSet<PaymentTypes> PaymentMethodsForCancellation = new()
        {
            PaymentTypes.CreditCard, PaymentTypes.Offline
        };

        private static readonly Expression<Func<Booking, bool>> IsBookingValidForCapturePredicate = booking
            => BookingStatusesForPayment.Contains(booking.Status) &&
            PaymentTypesForCapture.Contains(booking.PaymentType) &&
            booking.PaymentStatus == BookingPaymentStatuses.Authorized;

        private static readonly Expression<Func<Booking, bool>> IsBookingValidForChargePredicate = booking
            => BookingStatusesForPayment.Contains(booking.Status) &&
            PaymentTypesForCharge.Contains(booking.PaymentType) &&
            booking.PaymentStatus == BookingPaymentStatuses.NotPaid;

        private static readonly Expression<Func<Booking, bool>> IsBookingValidForDeadlineNotification = booking
            => BookingStatusesForPayment.Contains(booking.Status) &&
            PaymentStatusesForNotification.Contains(booking.PaymentStatus) &&
            booking.DeadlineNotificationSent == null;

        private static readonly HashSet<BookingStatuses> BookingStatusesForPayment = new()
        {
            BookingStatuses.Pending, BookingStatuses.Confirmed, BookingStatuses.WaitingForResponse
        };

        private static readonly HashSet<PaymentTypes> PaymentTypesForCapture = new()
        {
            PaymentTypes.CreditCard
        };

        private static readonly HashSet<PaymentTypes> PaymentTypesForCharge = new()
        {
            PaymentTypes.VirtualAccount
        };

        private static readonly HashSet<BookingPaymentStatuses> PaymentStatusesForNotification = new()
        {
            BookingPaymentStatuses.Authorized, BookingPaymentStatuses.NotPaid
        };

        private static readonly HashSet<BookingStatuses> BookingStatusesNeededRefreshBeforePayment = new()
        {
            BookingStatuses.Pending, BookingStatuses.WaitingForResponse
        };


        private readonly IBookingAccountPaymentService _accountPaymentService;
        private readonly IBookingCreditCardPaymentService _creditCardPaymentService;
        private readonly ISupplierBookingManagementService _supplierBookingManagementService;
        private readonly IBookingNotificationService _bookingNotificationService;
        private readonly IBookingReportsService _reportsService;
        private readonly EdoContext _context;
        private readonly IBookingRecordsUpdater _bookingRecordsUpdater;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}