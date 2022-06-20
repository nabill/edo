using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class Baggages
    {
        public static void AddBookingReferenceCode(string referenceCode)
        {
            Activity.Current?.AddBaggage("ReferenceCode", referenceCode);
        }
        
        public static void AddSearchId(Guid searchId)
        {
            Activity.Current?.AddBaggage("SearchId", searchId.ToString());
        }
    }
}