using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Analytics;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Messaging;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;
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
        public void LogWideAvailabilitySearch(in AgentContext agent) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.WideAvailabilitySearch,
                AgencyId = agent.AgencyId,
                AgentId = agent.AgentId,
                Country = null,
                Locality = null,
                Accommodation = null,
                SupplierCode = null,
                TotalPrice = null,
                GeoPoint = null
            });


        /// <summary>
        ///  Log accommodation availability event for analytics
        /// </summary>
        public void LogAccommodationAvailabilityRequested(in Accommodation accommodation, in AgentContext agent) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.AccommodationAvailabilitySearch,
                AgencyId = agent.AgencyId,
                AgentId = agent.AgentId,
                Country = accommodation.Location.Country,
                Locality = accommodation.Location.Country,
                Accommodation = accommodation.Name,
                SupplierCode = null,
                TotalPrice = null,
                GeoPoint = accommodation.Location.Coordinates
            });


        /// <summary>
        ///  Log booking occured event for analytics
        /// </summary>
        public void LogBookingOccured(Booking booking, in AgentContext agent) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.BookingOccured,
                AgencyId = agent.AgencyId,
                AgentId = agent.AgentId,
                Country = booking.Location.Country,
                Locality = booking.Location.Country,
                Accommodation = booking.AccommodationName,
                SupplierCode = booking.SupplierCode,
                TotalPrice = booking.TotalPrice,
                GeoPoint = new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude)
            });


        /// <summary>
        ///  Log booking confirmed event for analytics
        /// </summary>
        public void LogBookingConfirmed(Booking booking) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.BookingConfirmed,
                AgencyId = booking.AgencyId,
                AgentId = booking.AgentId,
                Country = booking.Location.Country,
                Locality = booking.Location.Country,
                Accommodation = booking.AccommodationName,
                SupplierCode = booking.SupplierCode,
                TotalPrice = booking.TotalPrice,
                GeoPoint = new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude)
            });


        /// <summary>
        ///  Log booking cancelled event for analytics
        /// </summary>
        public void LogBookingCancelled(Booking booking) 
            => Publish(new BookingAnalyticsEvent
            {
                DateTime = _dateTimeProvider.UtcNow(),
                EventId = (int) BookingAnalyticEventTypes.BookingCancelled,
                AgencyId = booking.AgencyId,
                AgentId = booking.AgentId,
                Country = booking.Location.Country,
                Locality = booking.Location.Country,
                Accommodation = booking.AccommodationName,
                SupplierCode = booking.SupplierCode,
                TotalPrice = booking.TotalPrice,
                GeoPoint = new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude)
            });


        private void Publish(BookingAnalyticsEvent @event) 
            => _messageBus.Publish(MessageBusTopics.BookingAnalyticsEvent, @event);


        private readonly IMessageBus _messageBus;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}