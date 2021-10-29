using System;
using System.Diagnostics;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class Baggage
    {
        public static void AddSearchId(Guid searchId)
        {
            Activity.Current?.AddBaggage("SearchId", searchId.ToString());
        }


        public static void AddBookingReferenceCode(string referenceCode)
        {
            Activity.Current?.AddBaggage("ReferenceCode", referenceCode);
        }
    }
}