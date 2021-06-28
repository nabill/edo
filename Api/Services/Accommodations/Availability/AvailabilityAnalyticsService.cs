using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Events;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.DataFormatters;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityAnalyticsService
    {
        public AvailabilityAnalyticsService(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }


        public void LogWideAvailabilitySearch(in AvailabilityRequest request, Guid searchId, IEnumerable<Location> locations, in AgentContext agent, string language)
        {
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
            
                _analytics.LogEvent(@event, "wide-availability-requested", agent, location.Coordinates);
            }
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
            
            var @event = new AccommodationBookingEvent(booking.AccommodationId,
                booking.AccommodationName,
                booking.Location.Country,
                booking.Location.Locality,
                adultsCount,
                childrenCount,
                (booking.CheckOutDate - booking.CheckInDate).Days,
                booking.Rooms.Count,
                bookingRequest.SearchId,
                bookingRequest.ResultId,
                bookingRequest.RoomContractSetId,
                booking.TotalPrice);
            
            _analytics.LogEvent(@event, "booking-request-sent", agent, booking.Location.Coordinates);
        }
        
        
        private const int AdultAge = 18;
        
        private readonly IAnalyticsService _analytics;
    }
}