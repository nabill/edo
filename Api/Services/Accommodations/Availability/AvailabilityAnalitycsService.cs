using System.Linq;
using HappyTravel.Edo.Api.Infrastructure.Analytics;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Analytics.Events;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Formatters;
using HappyTravel.Geography;

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
                location: GetLocationCoordinates(location.Coordinates));
            
            _analytics.LogEvent(@event, "wide-availability-requested");
        }


        public void LogAccommodationAvailabilityRequested(in AccommodationAvailabilityResult selectedResult, in AgentContext agent)
        {
            var @event = new AccommodationAvailabilityRequestEvent(id: selectedResult.Accommodation.Id,
                name: selectedResult.Accommodation.Name,
                counterpartyName: agent.CounterpartyName,
                rating: EnumFormatters.FromDescription(selectedResult.Accommodation.Rating),
                country: selectedResult.Accommodation.Location.Country,
                locality: selectedResult.Accommodation.Location.Locality,
                location: GetLocationCoordinates(selectedResult.Accommodation.Location.Coordinates));
            
            _analytics.LogEvent(@event, "accommodation-availability-requested");
        }
        
        
        private static float[] GetLocationCoordinates(GeoPoint point) 
            => new []{ (float)point.Latitude, (float)point.Longitude };
        
        private readonly IAnalyticsService _analytics;
    }
}