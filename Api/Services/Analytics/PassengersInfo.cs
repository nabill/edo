using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Analytics
{
    public class PassengersInfo
    {
        private PassengersInfo(int adultsCount, int childrenCount)
        {
            AdultsCount = adultsCount;
            ChildrenCount = childrenCount;
        }


        public static PassengersInfo FromRooms(List<BookedRoom> rooms)
        {
            var passengers = rooms.SelectMany(r => r.Passengers).ToList();

            var adultsCount = passengers.Count(p => p.Age is null || p.Age >= AdultAge);
            var childrenCount = passengers.Count(p => p.Age != null && p.Age < AdultAge);

            return new PassengersInfo(adultsCount, childrenCount);
        }


        public static PassengersInfo FromRoom(BookedRoom room)
        {
            var passengers = room.Passengers;

            var adultsCount = passengers.Count(p => p.Age is null || p.Age >= AdultAge);
            var childrenCount = passengers.Count(p => p.Age != null && p.Age < AdultAge);

            return new PassengersInfo(adultsCount, childrenCount);
        }


        public void Deconstruct(out int adultsCount, out int childrenCount)
        {
            adultsCount = AdultsCount;
            childrenCount = ChildrenCount;
        }


        private const int AdultAge = 18;
        private int AdultsCount { get; }
        private int ChildrenCount { get; }
    }
}