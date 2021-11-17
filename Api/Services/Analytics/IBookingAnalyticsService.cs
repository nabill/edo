using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;
using Accommodation = HappyTravel.MapperContracts.Public.Accommodations.Accommodation;

namespace HappyTravel.Edo.Api.Services.Analytics
{
    public interface IBookingAnalyticsService
    {
        public void LogWideAvailabilitySearch(in AvailabilityRequest request, Guid searchId, IEnumerable<Location> locations, in AgentContext agent,
            string language);
        public void LogAccommodationAvailabilityRequested(in Accommodation accommodation, Guid searchId, string htId, in AgentContext agent);
        public void LogBookingOccured(in AccommodationBookingRequest bookingRequest, Booking booking, in AgentContext agent);
        public void LogBookingConfirmed(Booking booking, string agencyName);
        public void LogBookingCancelled(Booking booking, string agencyName);
    }
}