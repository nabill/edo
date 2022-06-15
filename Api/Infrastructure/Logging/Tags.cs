using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class Tags
    {
        public static void AddBookingReferenceCode(string referenceCode)
        {
            Activity.Current?.AddTag("ReferenceCode", referenceCode);
        }
        
        public static void AddSearchId(Guid searchId)
        {
            Activity.Current?.AddTag("SearchId", searchId.ToString());
        }
    }
}