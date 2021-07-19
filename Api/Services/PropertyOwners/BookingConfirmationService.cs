using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.PropertyOwners;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public class BookingConfirmationService : IBookingConfirmationService
    {
        public BookingConfirmationService(EdoContext context, IBookingRecordManager bookingRecordManager, 
            IBookingRecordsUpdater recordsUpdater, IUrlGenerationService urlGenerationService, INotificationService notificationService,
            IOptions<PropertyOwnerMailingOptions> options)
        {
            _context = context;
            _bookingRecordManager = bookingRecordManager;
            _recordsUpdater = recordsUpdater;
            _urlGenerationService = urlGenerationService;
            _notificationService = notificationService;
            _options = options.Value;
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
                    _ => throw new NotImplementedException()
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
            var url = _urlGenerationService.Generate(booking.ReferenceCode);

            var bookingConfirmationData = new BookingConfirmationData
            { 
                ReferenceCode = booking.ReferenceCode,
                BookingConfirmationPageUrl = url
            };

            var email = "";

            return await _notificationService.Send(messageData: bookingConfirmationData,
                    notificationType: NotificationTypes.PropertyOwnerBookingConfirmation,
                    email: email,
                    templateId: _options.BookingConfirmationTemplateId);
        }


        private async Task<Result<Booking>> GetBooking(string referenceCode)
            => await _bookingRecordManager.Get(referenceCode);
 

        private bool IsDirectContract(Booking booking)
            => booking.IsDirectContract;


        private readonly EdoContext _context;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IBookingRecordsUpdater _recordsUpdater;
        private readonly IUrlGenerationService _urlGenerationService;
        private readonly INotificationService _notificationService;
        private readonly PropertyOwnerMailingOptions _options;
    }
}