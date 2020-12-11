using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Analytics.Events;
using HappyTravel.Edo.Api.Models.Availabilities;
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
            var @event = new WideAvailabilityRequestEvent(counterpartyName: agent.CounterpartyName,
                adultCount: request.RoomDetails.Sum(r => r.AdultsNumber),
                childrenCount: request.RoomDetails.Sum(r => r.ChildrenAges.Count),
                numberOfNights: (request.CheckOutDate - request.CheckInDate).Days,
                roomCount: request.RoomDetails.Count,
                country: location.Country,
                locality: location.Locality,
                locationName: location.Name,
                locationType: EnumFormatters.FromDescription(location.Type),
                coordinates: new []{ (float)location.Coordinates.Latitude, (float)location.Coordinates.Longitude });
            
            _analytics.LogEvent(@event, "wide-availability-requested");
        }


        public void LogAccommodationAvailabilityRequested(in AccommodationAvailabilityResult selectedResult, in AgentContext agent)
        {
            var @event = new AccommodationAvailabilityRequestEvent(selectedResult.Accommodation.Id,
                selectedResult.Accommodation.Name,
                agent.CounterpartyName);
            
            _analytics.LogEvent(@event, "accommodation-availability-requested");
        }
        
        
        private readonly IAnalyticsService _analytics;
    }
}