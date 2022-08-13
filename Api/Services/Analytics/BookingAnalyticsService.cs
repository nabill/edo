using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Analytics;
using HappyTravel.Edo.Api.Services.Messaging;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using Accommodation = HappyTravel.MapperContracts.Public.Accommodations.Accommodation;

namespace HappyTravel.Edo.Api.Services.Analytics
{
    public class BookingAnalyticsService : IBookingAnalyticsService
    {
        public BookingAnalyticsService(IMessageBus messageBus, IDateTimeProvider dateTimeProvider)
        {
            _messageBus = messageBus;
            _dateTimeProvider = dateTimeProvider;
        }

        /// <summary>
        ///  Log wide availability search event for analytics
        /// </summary>
        public void LogWideAvailabilitySearch(in AgentInfo agentInfo) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.WideAvailabilitySearch,
                AgencyId = agentInfo.AgencyId,
                AgentId = agentInfo.AgentId,
                Country = null,
                Locality = null,
                Accommodation = null,
                SupplierCode = null,
                TotalPrice = null,
                GeoPoint = null,
                AgencyName = agentInfo.AgencyName,
                AgentName = agentInfo.AgentName
            });


        /// <summary>
        ///  Log accommodation availability event for analytics
        /// </summary>
        public void LogAccommodationAvailabilityRequested(in Accommodation accommodation, in AgentInfo agentInfo) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.AccommodationAvailabilitySearch,
                AgencyId = agentInfo.AgencyId,
                AgentId = agentInfo.AgentId,
                Country = accommodation.Location.Country,
                Locality = accommodation.Location.Locality,
                Accommodation = accommodation.Name,
                SupplierCode = null,
                TotalPrice = null,
                GeoPoint = accommodation.Location.Coordinates,
                AgencyName = agentInfo.AgencyName,
                AgentName = agentInfo.AgentName
            });


        /// <summary>
        ///  Log booking occured event for analytics
        /// </summary>
        public void LogBookingOccured(Booking booking, in AgentInfo agentInfo) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.BookingOccured,
                AgencyId = agentInfo.AgencyId,
                AgentId = agentInfo.AgentId,
                Country = booking.Location.Country,
                Locality = booking.Location.Locality,
                Accommodation = booking.AccommodationName,
                SupplierCode = booking.SupplierCode,
                TotalPrice = booking.TotalPrice,
                GeoPoint = new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude),
                AgencyName = agentInfo.AgencyName,
                AgentName = agentInfo.AgentName
            });


        /// <summary>
        ///  Log booking confirmed event for analytics
        /// </summary>
        public void LogBookingConfirmed(Booking booking, in AgentInfo agentInfo) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.BookingConfirmed,
                AgencyId = agentInfo.AgencyId,
                AgentId = agentInfo.AgentId,
                Country = booking.Location.Country,
                Locality = booking.Location.Locality,
                Accommodation = booking.AccommodationName,
                SupplierCode = booking.SupplierCode,
                TotalPrice = booking.TotalPrice,
                GeoPoint = new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude),
                AgencyName = agentInfo.AgencyName,
                AgentName = agentInfo.AgentName
            });


        /// <summary>
        ///  Log booking cancelled event for analytics
        /// </summary>
        public void LogBookingCancelled(Booking booking, in AgentInfo agentInfo) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.BookingCancelled,
                AgencyId = agentInfo.AgencyId,
                AgentId = agentInfo.AgentId,
                Country = booking.Location.Country,
                Locality = booking.Location.Locality,
                Accommodation = booking.AccommodationName,
                SupplierCode = booking.SupplierCode,
                TotalPrice = booking.TotalPrice,
                GeoPoint = new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude),
                AgencyName = agentInfo.AgencyName,
                AgentName = agentInfo.AgentName
            });


        private void Publish(BookingAnalyticsEvent @event) 
            => _messageBus.Publish(MessageBusTopics.BookingAnalyticsEvent, @event);


        private readonly IMessageBus _messageBus;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}