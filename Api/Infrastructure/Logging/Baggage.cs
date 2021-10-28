using System;
using System.Diagnostics;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class Baggage
    {
        public static void SetSearchId(Guid searchId)
        {
            Activity.Current?.AddBaggage("SearchId", searchId.ToString());
        }
    }
}