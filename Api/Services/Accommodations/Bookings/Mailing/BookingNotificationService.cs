using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EnumFormatters = HappyTravel.DataFormatters.EnumFormatters;
using MoneyFormatter = HappyTravel.DataFormatters.MoneyFormatter;
using DateTimeFormatters = HappyTravel.DataFormatters.DateTimeFormatters;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public class BookingNotificationService : IBookingNotificationService
    {
        public BookingNotificationService(IBookingRecordManager bookingRecordManager, 
            INotificationService notificationService,
            IOptions<BookingMailingOptions> options,
            EdoContext context)
        {
            _bookingRecordManager = bookingRecordManager;
            _notificationService = notificationService;
            _options = options.Value;
            _context = context;
        }


        public async Task NotifyBookingCancelled(AccommodationBookingInfo bookingInfo, SlimAgentContext agent)
        {
            await SendDetailedBookingNotification(bookingInfo, bookingInfo.AgentInformation.AgentEmail, agent, NotificationTypes.BookingCancelled);
            
            await SendDetailedBookingNotification(bookingInfo, _options.CcNotificationAddresses, NotificationTypes.BookingCancelled);
        }


        // TODO: hardcoded to be removed with UEDA-20
        public async Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo, SlimAgentContext agent)
        {
            await SendDetailedBookingNotification(bookingInfo, bookingInfo.AgentInformation.AgentEmail, agent, NotificationTypes.BookingFinalized);
            
            await SendDetailedBookingNotification(bookingInfo, _options.CcNotificationAddresses, NotificationTypes.BookingFinalized);
        }


        public Task<Result> NotifyDeadlineApproaching(int bookingId, string email)
        {
            return _bookingRecordManager.Get(bookingId)
                .Bind(async booking => 
                {
                    var roomDescriptions = booking.Rooms
                        .Select(r => r.ContractDescription);

                    var passengers = booking.Rooms
                        .SelectMany(r => r.Passengers)
                        .Select(p => $"{p.FirstName} {p.LastName}");
                    
                    var deadlineData = new BookingDeadlineData
                    {
                        BookingId = booking.Id,
                        RoomDescriptions = string.Join(", ", roomDescriptions),
                        Passengers = string.Join(", ", passengers),
                        ReferenceCode = booking.ReferenceCode,
                        CheckInDate = DateTimeFormatters.ToDateString(booking.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(booking.CheckOutDate),
                        Deadline = DateTimeFormatters.ToDateString(booking.DeadlineDate)
                    };

                    return await _notificationService.Send(agent: new SlimAgentContext(agentId: booking.AgentId, agencyId: booking.AgencyId),
                                messageData: deadlineData,
                                notificationType: NotificationTypes.DeadlineApproaching,
                                email: email);
                });
        }


        public async Task<Result> NotifyCreditCardPaymentConfirmed(string referenceCode)
        {
            return await GetData()
                .Tap(SendNotificationToAdmin)
                .Tap(SendNotificationToAgent);


            async Task<Result<CreditCardPaymentConfirmationNotification>> GetData()
            {
                var query = from booking in _context.Bookings
                    join agent in _context.Agents on booking.AgentId equals agent.Id
                    join agentAgencyRelation in _context.AgentAgencyRelations on agent.Id equals agentAgencyRelation.AgentId
                    join agency in _context.Agencies on agentAgencyRelation.AgencyId equals agency.Id
                    where booking.ReferenceCode == referenceCode
                    select new CreditCardPaymentConfirmationNotification
                    {
                        Agency = agency.Name,
                        Agent = $"{agent.FirstName} {agent.LastName}",
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = booking.GetLeadingPassengerFormattedName(),
                        Amount = MoneyFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = DateTimeFormatters.ToDateString(booking.DeadlineDate),
                        CheckInDate = DateTimeFormatters.ToDateString(booking.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(booking.CheckOutDate),
                        Status = EnumFormatters.FromDescription(booking.Status),
                        PaymentStatus = EnumFormatters.FromDescription(booking.PaymentStatus),
                        Email = agent.Email
                    };

                var data = await query.SingleOrDefaultAsync();

                return data ?? Result.Failure<CreditCardPaymentConfirmationNotification>($"Booking with reference code {referenceCode} not found");
            }


            Task SendNotificationToAdmin(CreditCardPaymentConfirmationNotification data)
                =>  _notificationService.Send(messageData: data,
                    notificationType: NotificationTypes.CreditCardPaymentReceivedAdministrator,
                    emails: _options.CcNotificationAddresses);


            async Task SendNotificationToAgent(CreditCardPaymentConfirmationNotification data)
            {
                var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == data.ReferenceCode);

                await _notificationService.Send(agent: new SlimAgentContext(booking.AgentId, booking.AgencyId),
                    messageData: data,
                    notificationType: NotificationTypes.CreditCardPaymentReceived,
                    email: data.Email);
            }
        }


        public async Task NotifyBookingManualCorrectionNeeded(string referenceCode, string agentName, string agencyName, string deadline)
        {
            var data = new BookingManualCorrectionNeededData
            {
                ReferenceCode = referenceCode,
                AgentName = agentName,
                AgencyName = agencyName,
                Deadline = deadline
            };

            await _notificationService.Send(messageData: data,
                notificationType: NotificationTypes.BookingManualCorrectionNeeded,
                emails: _options.CcNotificationAddresses);
        }


        public async Task NotifyAdminsStatusChanged(AccommodationBookingInfo bookingInfo, SlimAgentContext agent)
        {
            // TODO: remove when we have appropriate admin panel booking monitoring
            await SendDetailedBookingNotification(bookingInfo, _options.CcNotificationAddresses, NotificationTypes.BookingStatusChanged);
        }


        private Task SendDetailedBookingNotification(AccommodationBookingInfo bookingInfo, string recipient, SlimAgentContext agent, 
            NotificationTypes notificationType)
        {
            var details = bookingInfo.BookingDetails;
            var notificationData = CreateNotificationData(bookingInfo, details);

            return _notificationService.Send(agent: agent,
                messageData: notificationData,
                notificationType: notificationType,
                email: recipient);
        }


        private Task SendDetailedBookingNotification(AccommodationBookingInfo bookingInfo, List<string> recipients, NotificationTypes notificationType)
        {
            var details = bookingInfo.BookingDetails;
            var notificationData = CreateNotificationData(bookingInfo, details);

            return _notificationService.Send(messageData: notificationData,
                notificationType: notificationType,
                emails: recipients);
        }


        private static BookingNotificationData CreateNotificationData(AccommodationBookingInfo bookingInfo, AccommodationBookingDetails details)
        {
            return new BookingNotificationData
            {
                AgentName = bookingInfo.AgentInformation.AgentName,
                BookingDetails = new BookingNotificationData.Details
                {
                    AccommodationName = details.AccommodationName,
                    CheckInDate = DateTimeFormatters.ToDateString(details.CheckInDate),
                    CheckOutDate = DateTimeFormatters.ToDateString(details.CheckOutDate),
                    DeadlineDate = DateTimeFormatters.ToDateString(details.DeadlineDate),
                    Location = details.Location,
                    NumberOfNights = details.NumberOfNights,
                    NumberOfPassengers = details.NumberOfPassengers,
                    ReferenceCode = details.ReferenceCode,
                    RoomDetails = details.RoomDetails.Select(d =>
                    {
                        var maskedPassengers = d.Passengers.Where(p => p.IsLeader)
                            .Select(p =>
                            {
                                var firstName = p.FirstName.Length == 1 ? "*" : p.FirstName.Substring(0, 1);
                                return new Pax(p.Title, p.LastName, firstName);
                            })
                            .ToList();

                        return new BookingNotificationData.BookedRoomDetails
                        {
                            ContractDescription = d.ContractDescription,
                            MealPlan = d.MealPlan,
                            Passengers = maskedPassengers,
                            Price = MoneyFormatter.ToCurrencyString(d.Price.Amount, d.Price.Currency),
                            Type = EnumFormatters.FromDescription(d.Type),
                            Remarks = d.Remarks
                        };
                    }).ToList(),
                    Status = EnumFormatters.FromDescription(details.Status),
                    SupplierReferenceCode = details.AgentReference,
                    ContactInfo = details.ContactInfo,
                },
                AgencyName = bookingInfo.AgentInformation.AgencyName,
                PaymentStatus = EnumFormatters.FromDescription(bookingInfo.PaymentStatus),
                Price = MoneyFormatter.ToCurrencyString(bookingInfo.TotalPrice.Amount, bookingInfo.TotalPrice.Currency),
                Supplier = bookingInfo.Supplier is null
                    ? string.Empty
                    : EnumFormatters.FromDescription(bookingInfo.Supplier.Value),
            };
        }


        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly INotificationService _notificationService;
        private readonly BookingMailingOptions _options;
        private readonly EdoContext _context;
    }
}