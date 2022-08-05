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
        public void LogWideAvailabilitySearch(in AgentContext agent);
        public void LogAccommodationAvailabilityRequested(in Accommodation accommodation, in AgentContext agent);
        public void LogBookingOccured(Booking booking, in AgentContext agent);
        public void LogBookingConfirmed(Booking booking);
        public void LogBookingCancelled(Booking booking);
    }
}