using System;
using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Events;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Formatters;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityAnalyticsService
    {
        public AvailabilityAnalyticsService(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }


        public void LogWideAvailabilitySearch(in AvailabilityRequest request, Guid searchId, in Location location, in AgentContext agent, string language)
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
            
            _analytics.LogEvent(@event, "wide-availability-requested", agent, location.Coordinates);
        }


        public void LogAccommodationAvailabilityRequested(in AccommodationAvailabilityResult selectedResult, Guid searchId, Guid resultId, in AgentContext agent)
        {
            var accommodation = selectedResult.Accommodation;
            var @event = new AccommodationAvailabilityRequestEvent(id: accommodation.Id,
                name: accommodation.Name,
                rating: EnumFormatters.FromDescription(accommodation.Rating),
                country: accommodation.Location.Country,
                locality: accommodation.Location.Locality,
                searchId: searchId,
                resultId: resultId);
            
            _analytics.LogEvent(@event, "accommodation-availability-requested", agent, accommodation.Location.Coordinates);
        }
        

        public void LogBookingOccured(in AccommodationBookingRequest bookingRequest, Booking booking,
            in AgentContext agent)
        {
            var passengers = bookingRequest.RoomDetails.SelectMany(r => r.Passengers).ToList();
            var adultsCount = passengers.Count(p => p.Age != null && p.Age >= AdultAge);
            var childrenCount = passengers.Count(p => p.Age != null && p.Age < AdultAge);
            var nightsCount = (booking.CheckOutDate - booking.CheckInDate).Days;
            
            var @event = new AccommodationBookingEvent(accommodationId: booking.AccommodationId,
                accommodationName: booking.AccommodationName,
                country: booking.Location.Country,
                locality: booking.Location.Locality,
                adultCount: adultsCount,
                childrenCount: childrenCount,
                numberOfNights: (booking.CheckOutDate - booking.CheckInDate).Days,
                roomCount: booking.Rooms.Count,
                bookingRequest.SearchId,
                bookingRequest.ResultId,
                bookingRequest.RoomContractSetId,
                nightsCount);
            
            _analytics.LogEvent(@event, "booking-request-sent", agent, booking.Location.Coordinates);
        }
        
        
        private const int AdultAge = 18;
        
        private readonly IAnalyticsService _analytics;
    }
}