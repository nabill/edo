using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Analytics;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using HappyTravel.SupplierOptionsProvider;
using Accommodation = HappyTravel.MapperContracts.Public.Accommodations.Accommodation;

namespace HappyTravel.Edo.Api.Services.Analytics
{
    public class BookingAnalyticsService : IBookingAnalyticsService
    {
        public BookingAnalyticsService(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }


        public void LogWideAvailabilitySearch(in AvailabilityRequest request, Guid searchId, IEnumerable<Location> locations, in AgentContext agent,
            string language)
        {
            var agentAnalyticsInfo = new AgentAnalyticsInfo(agent.AgencyName);
            foreach (var location in locations)
            {
                var @event = new WideAvailabilityRequestEvent(adultCount: request.RoomDetails.Sum(r => r.AdultsNumber),
                    childrenCount: request.RoomDetails.Sum(r => r.ChildrenAges.Count),
                    numberOfNights: (request.CheckOutDate - request.CheckInDate).Days,
                    roomCount: request.RoomDetails.Count,
                    country: location.Country,
                    locality: location.Locality,
                    locationName: location.Name,
                    locationType: EnumFormatters.FromDescription(location.Type),
                    searchId: searchId,
                    language);

                _analytics.LogEvent(@event, "wide-availability-requested", agentAnalyticsInfo, location.Coordinates);
            }
        }


        public void LogAccommodationAvailabilityRequested(in Accommodation accommodation, Guid searchId, string htId, in AgentContext agent)
        {
            var agentAnalyticsInfo = new AgentAnalyticsInfo(agent.AgencyName);
            var @event = new AccommodationAvailabilityRequestEvent(name: accommodation.Name,
                rating: EnumFormatters.FromDescription(accommodation.Rating),
                country: accommodation.Location.Country,
                locality: accommodation.Location.Locality,
                searchId: searchId,
                htId: htId);

            _analytics.LogEvent(@event, "accommodation-availability-requested", agentAnalyticsInfo, accommodation.Location.Coordinates);
        }


        public void LogBookingOccured(in AccommodationBookingRequest bookingRequest, Booking booking, in AgentContext agent)
        {
            var (adultsCount, childrenCount) = PassengersInfo.FromRooms(booking.Rooms);
            var agentAnalyticsInfo = new AgentAnalyticsInfo(agent.AgencyName);

            var @event = new AccommodationBookingEvent(booking.AccommodationId,
                booking.AccommodationName,
                booking.Location.Country,
                booking.Location.Locality,
                adultsCount,
                childrenCount,
                (booking.CheckOutDate - booking.CheckInDate).Days,
                booking.Rooms.Count,
                bookingRequest.SearchId,
                bookingRequest.HtId,
                bookingRequest.RoomContractSetId,
                booking.TotalPrice,
                booking.SupplierCode);

            _analytics.LogEvent(@event, "booking-request-sent", agentAnalyticsInfo,
                new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude));
        }


        public void LogBookingConfirmed(Booking booking, string agencyName)
        {
            var (adultsCount, childrenCount) = PassengersInfo.FromRooms(booking.Rooms);
            var agentAnalyticsInfo = new AgentAnalyticsInfo(agencyName);

            var @event = new BookingConfirmationEvent(booking.AccommodationId,
                booking.AccommodationName,
                booking.Location.Country,
                booking.Location.Locality,
                adultsCount,
                childrenCount,
                (booking.CheckOutDate - booking.CheckInDate).Days,
                booking.Rooms.Count,
                booking.HtId,
                booking.TotalPrice,
                booking.SupplierCode);

            _analytics.LogEvent(@event, "booking-confirmed", agentAnalyticsInfo,
                new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude));
        }


        public void LogBookingCancelled(Booking booking, string agencyName)
        {
            var (adultsCount, childrenCount) = PassengersInfo.FromRooms(booking.Rooms);
            var agentAnalyticsInfo = new AgentAnalyticsInfo(agencyName);

            var @event = new BookingCancellationEvent(booking.AccommodationId,
                booking.AccommodationName,
                booking.Location.Country,
                booking.Location.Locality,
                adultsCount,
                childrenCount,
                (booking.CheckOutDate - booking.CheckInDate).Days,
                booking.Rooms.Count,
                booking.HtId,
                booking.TotalPrice,
                booking.SupplierCode);

            _analytics.LogEvent(@event, "booking-cancelled", agentAnalyticsInfo,
                new GeoPoint(booking.Location.Coordinates.Longitude, booking.Location.Coordinates.Latitude));
        }


        private readonly IAnalyticsService _analytics;
    }
}