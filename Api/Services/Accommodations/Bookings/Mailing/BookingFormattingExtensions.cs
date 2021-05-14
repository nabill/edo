using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.DataFormatters;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public static class BookingFormattingExtensions
    {
        public static string GetLeadingPassengerFormattedName(this Booking booking)
        {
            var leadingPassengersList = booking.Rooms
                .SelectMany(r =>
                {
                    if (r.Passengers == null)
                        return new List<Passenger>(0);
                    
                    return r.Passengers.Where(p => p.IsLeader);
                })
                .ToList();
            
            if (leadingPassengersList.Any())
            {
                var leadingPassenger = leadingPassengersList.First();
                return PersonNameFormatters.ToMaskedName(leadingPassenger.FirstName, leadingPassenger.LastName,
                    EnumFormatters.FromDescription(leadingPassenger.Title));
            }

            return PersonNameFormatters.ToMaskedName("*", string.Empty);
        }
    }
}