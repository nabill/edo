using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.PropertyOwners;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public class BookingConfirmationService : IBookingConfirmationService
    {
        public BookingConfirmationService(EdoContext context, IBookingRecordManager bookingRecordManager, 
            IBookingRecordsUpdater recordsUpdater, IPropertyOwnerConfirmationUrlGenerator urlGenerationService, INotificationService notificationService,
            IOptions<PropertyOwnerMailingOptions> options, IAccommodationMapperClient client, ILogger<BookingConfirmationService> logger,
            IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _bookingRecordManager = bookingRecordManager;
            _recordsUpdater = recordsUpdater;
            _urlGenerationService = urlGenerationService;
            _notificationService = notificationService;
            _options = options.Value;
            _client = client;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }


        public async Task<Result<SlimBookingConfirmation>> Get(string referenceCode)
        {
            return await GetBooking(referenceCode)
                .Ensure(IsDirectContract, $"Booking with the reference code '{referenceCode}' is not a direct contract")
                .Map(ConvertToSlimBookingConfirmation);


            static SlimBookingConfirmation ConvertToSlimBookingConfirmation(Booking booking)
                => new()
                {
                    ReferenceCode = booking.ReferenceCode,
                    ConfirmationCode = booking.PropertyOwnerConfirmationCode,
                    Status = booking.Status
                };
        }


        public async Task<Result> Update(string referenceCode, BookingConfirmation bookingConfirmation)
        {
            return await GetBooking(referenceCode)
                .Ensure(IsDirectContract, $"Booking with the reference code '{referenceCode}' is not a direct contract")
                .BindWithTransaction(_context, booking => UpdateBooking(booking)
                    .Tap(SendStatusToPms)
                    .Tap(SaveHistory));


            async Task<Result> UpdateBooking(Booking booking)
            {
                if (bookingConfirmation.ConfirmationCode != string.Empty)
                    await _recordsUpdater.ChangePropertyOwnerConfirmationCode(booking: booking,
                        confirmationCode: bookingConfirmation.ConfirmationCode);

                var newStatus = bookingConfirmation.Status switch
                {
                    BookingConfirmationStatuses.OnRequest => BookingStatuses.Pending,
                    BookingConfirmationStatuses.Amended => BookingStatuses.ManualCorrectionNeeded,
                    BookingConfirmationStatuses.Confirmed => BookingStatuses.Confirmed,
                    BookingConfirmationStatuses.Cancelled => BookingStatuses.Cancelled,
                    _ => throw new NotImplementedException("No corresponding booking status has been defined for the specified booking confirmation status")
                };

                return await _recordsUpdater.ChangeStatus(booking: booking, 
                    status: newStatus,
                    date: bookingConfirmation.CreatedAt, 
                    apiCaller: Models.Users.ApiCaller.InternalServiceAccount, 
                    reason: new BookingChangeReason 
                    {
                        Source = BookingChangeSources.PropertyOwner,
                        Event = BookingChangeEvents.BookingConfirmation,
                        Reason = $"Status changed by property owner employee {bookingConfirmation.Initiator}"
                    });
            }


            Task SendStatusToPms()
            {
                // TODO: Sending the hotel's changed booking status to PMS (Columbus) will be implemented in task AA-415
                return Task.CompletedTask;
            }


            Task SaveHistory()
            {
                _context.BookingConfirmationHistory.Add(new BookingConfirmationHistoryEntry
                {
                    ReferenceCode = referenceCode,
                    ConfirmationCode = bookingConfirmation.ConfirmationCode,
                    Status = bookingConfirmation.Status,
                    Comment = bookingConfirmation.Comment,
                    Initiator = bookingConfirmation.Initiator,
                    CreatedAt = bookingConfirmation.CreatedAt
                });

                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> SendConfirmationEmail(Booking booking)
        {
            if (_hostingEnvironment.IsProduction()) // TODO: This check will be removed after testing the hotel confirmation page in development
                return Result.Success();

            if (!booking.IsDirectContract || booking.Supplier != ColumbusSupplierId)
                return Result.Success(); 
            
            var url = _urlGenerationService.Generate(booking.ReferenceCode);

            var roomDetails = new List<BookingConfirmationData.BookedRoomDetails>();
            foreach (var room in booking.Rooms)
            {
                var passengers = room.Passengers
                    .Where(p => p.IsLeader)
                    .Select(p => $"{p.Title} {p.LastName} {p.FirstName}")
                    .ToList();

                roomDetails.Add(new BookingConfirmationData.BookedRoomDetails 
                {
                    MainPassengers = String.Join(", ", passengers),
                    Type = EnumFormatters.FromDescription(room.Type),
                    PromoCode = "", // TODO: Need clarify this
                    Price = MoneyFormatter.ToCurrencyString(room.Price.Amount, room.Price.Currency),
                    MealPlan = room.MealPlan,
                    NumberOfPassengers = BuildPassengersString(room.Passengers),
                    ContractDescription = room.ContractDescription,
                });
            }

            var bookingConfirmationData = new BookingConfirmationData
            { 
                ReferenceCode = booking.ReferenceCode,
                AccommodationName = booking.AccommodationName,
                CheckInDate = DateTimeFormatters.ToDateString(booking.CheckInDate),
                CheckOutDate = DateTimeFormatters.ToDateString(booking.CheckOutDate),
                RoomDetails = roomDetails,
                BookingConfirmationPageUrl = url
            };

            var (_, isFailure, emails, error) = await _client.GetAccommodationEmails(booking.HtId);
            if (isFailure)
                return Result.Failure(error.Detail);

            if (emails.Count == 0)
            {
                _logger.LogSendConfirmationEmailFailure(booking.ReferenceCode);
                return Result.Failure("Missing email address to send email to property owner");
            }

            return await _notificationService.Send(apiCaller: new Models.Users.ApiCaller(id: default, type: ApiCallerTypes.PropertyOwner),
                    messageData: bookingConfirmationData,
                    notificationType: NotificationTypes.PropertyOwnerBookingConfirmation,
                    emails: new List<string> { emails.First(), _options.ReservationsOfficeBackupEmail });
                    // We only use the first email because: 1) Hotel email from Columbus will be returned first;
                    // 2) In the current implementation of the mapper, we do not know which provider returns the second email address to us,
                    // so we cannot use it.
                    // TODO: After Notification Center refactoring ReservationsOfficeBackupEmail will be moved to the copy.


            static string BuildPassengersString(List<Passenger> passengers)
            {
                var adult = 0;
                var children = 0;
                var childrenStr = string.Empty;

                foreach (var passenger in passengers)
                {
                    if (passenger.Age >= MinimumAdultAge)
                        adult++;
                    else
                    {
                        children++;
                        childrenStr += $", {passenger.Age} year";
                        if (passenger.Age > 1)
                            childrenStr += "s";
                    }
                }

                var result = (adult == 1) 
                    ? $"{adult} adult" 
                    : $"{adult} adults";

                if (children == 1)
                    result += $"{childrenStr} child";
                else if (children > 1)
                    result += $"{childrenStr} children";
                
                return result;
            }
        }


        private async Task<Result<Booking>> GetBooking(string referenceCode)
            => await _bookingRecordManager.Get(referenceCode);
 

        private bool IsDirectContract(Booking booking)
            => booking.IsDirectContract;


        private const int MinimumAdultAge = 18;
        private const int ColumbusSupplierId = 6;

        private readonly EdoContext _context;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IBookingRecordsUpdater _recordsUpdater;
        private readonly IPropertyOwnerConfirmationUrlGenerator _urlGenerationService;
        private readonly INotificationService _notificationService;
        private readonly PropertyOwnerMailingOptions _options;
        private readonly IAccommodationMapperClient _client;
        private readonly ILogger<BookingConfirmationService> _logger;
        private readonly IHostEnvironment _hostingEnvironment;
    }
}