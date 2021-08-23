using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HappyTravel.Edo.Api.Services.Analytics
{
    public readonly struct PassengersInfo
    {
        public PassengersInfo(List<BookedRoom> rooms)
        {
            var passengers = rooms.SelectMany(r => r.Passengers).ToList();
            AdultsCount = passengers.Count(p => p.Age is >= AdultAge);
            ChildrenCount = passengers.Count(p => p.Age is < AdultAge);
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