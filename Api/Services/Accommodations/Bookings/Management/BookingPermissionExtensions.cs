using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public static class BookingPermissionExtensions
    {
        public static Result<Booking> CheckPermissions(this Booking booking, AgentContext agent)
        {
            return DoesAgentHavePermissions(booking, agent)
                ? booking
                : Result.Failure<Booking>("Permission denied");
        }
        
        
        public static Result<Booking> CheckPermissions(this Result<Booking> bookingResult, AgentContext agent)
        {
            if (bookingResult.IsFailure)
                return bookingResult;

            return CheckPermissions(bookingResult.Value, agent);
        }
        
        
        public static async Task<Result<Booking>> CheckPermissions(this Task<Result<Booking>> bookingResultTask, AgentContext agent) 
            => CheckPermissions(await bookingResultTask, agent);


        private static bool DoesAgentHavePermissions(Booking booking, AgentContext agent)
        {
            if (booking.AgencyId != agent.AgencyId)
                return false;

            if (booking.AgentId == agent.AgentId)
                return true;

            return agent.InAgencyPermissions.HasFlag(InAgencyPermissions.AgencyBookingsManagement);
        }
    }
}