using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Events;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Formatters;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityAnalyticsService
    {
        public AvailabilityAnalyticsService(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }


        public void LogWideAvailabilitySearch(in AvailabilityRequest request, in Location location, in AgentContext agent)
        {
            var @event = new WideAvailabilityRequestEvent(adultCount: request.RoomDetails.Sum(r => r.AdultsNumber),
                childrenCount: request.RoomDetails.Sum(r => r.ChildrenAges.Count),
                numberOfNights: (request.CheckOutDate - request.CheckInDate).Days,
                roomCount: request.RoomDetails.Count,
                country: location.Country,
                locality: location.Locality,
                locationName: location.Name,
                locationType: EnumFormatters.FromDescription(location.Type));
            
            _analytics.LogEvent(@event, "wide-availability-requested", agent, location.Coordinates);
        }


        public void LogAccommodationAvailabilityRequested(in AccommodationAvailabilityResult selectedResult, in AgentContext agent)
        {
            var accommodation = selectedResult.Accommodation;
            var @event = new AccommodationAvailabilityRequestEvent(id: accommodation.Id,
                name: accommodation.Name,
                rating: EnumFormatters.FromDescription(accommodation.Rating),
                country: accommodation.Location.Country,
                locality: accommodation.Location.Locality);
            
            _analytics.LogEvent(@event, "accommodation-availability-requested", agent, accommodation.Location.Coordinates);
        }
        
        
        private readonly IAnalyticsService _analytics;
    }
}