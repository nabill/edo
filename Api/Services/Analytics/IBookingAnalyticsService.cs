using HappyTravel.Edo.Api.Models.Analytics;
using HappyTravel.Edo.Data.Bookings;
using Accommodation = HappyTravel.MapperContracts.Public.Accommodations.Accommodation;

namespace HappyTravel.Edo.Api.Services.Analytics
{
    public interface IBookingAnalyticsService
    {
        public void LogWideAvailabilitySearch(in AgentInfo agentInfo);
        public void LogAccommodationAvailabilityRequested(in Accommodation accommodation, in AgentInfo agentInfo);
        public void LogBookingOccured(Booking booking, in AgentInfo agentInfo);
        public void LogBookingConfirmed(Booking booking, in AgentInfo agentInfo);
        public void LogBookingCancelled(Booking booking, in AgentInfo agentInfo);
    }
}